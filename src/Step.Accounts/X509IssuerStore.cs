using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Step.Accounts.Extensions;

namespace Step.Accounts
{
    public class X509IssuerStore : IX509IssuerStore
    {
        public X509IssuerStore(
            AccountOptions accountOptions,
            X509IssuerStoreOptions storeOptions,
            ILoggerFactory mill
        )
        {
            _options = accountOptions;
            _storeOptions = storeOptions;
            _logger = mill.CreateLogger<X509IssuerStore>();
            _path = Path.Combine(_storeOptions.RootPath, _options.Registration.IssuerCertificatesPath);
            Load();
        }

        private readonly AccountOptions _options;
        private readonly X509IssuerStoreOptions _storeOptions;
        private readonly string _path;
        private readonly ILogger _logger;
        private Dictionary<string, X509Certificate2> _certs = new Dictionary<string, X509Certificate2>();

        public List<X509Certificate2> Certificates
        {
            get { return _certs.Values.ToList(); }
            set { _certs = value.ToDictionary(c => c.Subject); }
        }

        public void Add(X509Certificate2 certificate)
        {
            if (!_certs.ContainsKey(certificate.Subject))
                _certs.Add(certificate.Subject, certificate);
        }

        public void Load()
        {
            int count = 0;
            if (_path.HasValue() && Directory.Exists(_path))
            {
                _certs.Clear();
                foreach (string file in Directory.GetFiles(_path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        _logger.LogDebug("Loading certificate {1}", file);
                        X509Certificate2 certificate = new X509Certificate2(file);
                        if (!_certs.ContainsKey(certificate.Subject))
                            _certs.Add(certificate.Subject, certificate);
                        count += 1;
                    }
                    catch
                    {
                        _logger.LogDebug("Failed to load {1}", file);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Invalid path [{1}]", _path);
            }
        }

        public bool HasIssuer(string issuer)
        {
            return _certs.ContainsKey(issuer);
        }

        public bool Validate(X509Certificate2 certificate)
        {
            _logger.LogDebug("Validating certificate {1}", certificate.Subject);

            if (!_certs.ContainsKey(certificate.Issuer))
            {
                _logger.LogDebug("Issuer not trusted: {1}", certificate.Issuer);
                throw new ArgumentException("Certificate issuer not trusted.", "certificate");
            }

            X509Chain chain = new X509Chain();
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 10);
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EndCertificateOnly; //.ExcludeRoot;
            chain.ChainPolicy.RevocationMode = (_options.Authentication.CheckCertificateRevocation)
                ? X509RevocationMode.Online
                : X509RevocationMode.NoCheck;

            foreach (X509Certificate2 cert in _certs.Values)
                chain.ChainPolicy.ExtraStore.Add(cert);

            chain.Build(certificate);

            List<X509ChainStatus> stati = chain.ChainStatus.ToList();
            foreach (X509ChainStatus status in stati.ToArray())
            {
                switch (status.Status)
                {
                    case X509ChainStatusFlags.UntrustedRoot:
                        //the app supplied the approved issuers, so it trusts them.
                        if (_certs.ContainsKey(certificate.Issuer))
                            stati.Remove(status);
                        break;
                }
            }

            if (stati.Count > 0)
            {
                string msg = String.Join(", ", stati.Select(x => x.StatusInformation));
                _logger.LogDebug("Failed to validate {1}: {2}", certificate.Subject, msg);
                throw new ArgumentException(msg, "certificate");
            }

            _logger.LogDebug("Validated certificate " + certificate.Subject);
            return true;
        }
    }
}