using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Models.Virtual;
using TopoMojo.vSphere.Helpers;

namespace TopoMojo.vSphere.Network
{
    public class VlanManager
    {

        public VlanManager (
            VlanOptions options
        ) {
            _options = options;
            InitVlans();
        }

        protected VlanOptions _options;
        protected Dictionary<string, int> _vlans;
        protected BitArray _vlanMap;

        private void InitVlans()
        {
            //initialize vlan map
            _vlanMap = new BitArray(4096, true);
            foreach (int i in _options.Range.ExpandRange())
            {
                _vlanMap[i] = false;
            }

            //set admin reservations
            _vlans = new Dictionary<string,int>();
            foreach (Vlan vlan in _options.Reservations)
            {
                _vlans.Add(vlan.Name, vlan.Id);
                _vlanMap[vlan.Id] = true;
            }
        }

        public bool Contains(string net)
        {
            return _vlans.ContainsKey(net);
        }

        public void Activate(Vlan[] vlans)
        {
            lock(_vlanMap)
            {
                foreach (Vlan vlan in vlans)
                {
                    _vlanMap[vlan.Id] = true;
                    if (!_vlans.ContainsKey(vlan.Name))
                        _vlans.Add(vlan.Name, vlan.Id);
                }
            }
        }

        public void Deactivate(string net)
        {
            //only deallocate tagged nets
            if (!net.Contains("#"))
                return;

            lock(_vlanMap)
            {
                if (_vlans.ContainsKey(net))
                {
                    _vlanMap[_vlans[net]] = false;
                    _vlans.Remove(net);
                }
            }
        }

        public virtual void ReserveVlans(Template template)
        {
            lock (_vlanMap)
            {
                foreach (Eth eth in template.Eth)
                {
                    //if net already reserved, use reserved vlan
                    if (_vlans.ContainsKey(eth.Net))
                    {
                        eth.Vlan = _vlans[eth.Net];
                    }
                    else
                    {
                        int id = 0;
                        if (template.UseUplinkSwitch)
                        {
                            //get available uplink vlan
                            while (id < _vlanMap.Length && _vlanMap[id])
                            {
                                id += 1;
                            }

                            if (id > 0 && id < _vlanMap.Length)
                            {
                                eth.Vlan = id;
                                _vlanMap[id] = true;
                                _vlans.Add(eth.Net, id);
                            }
                            else
                            {
                                throw new Exception("Unable to reserve a vlan for " + eth.Net);
                            }
                        }
                        else {
                            //get highest vlan in this isolation group
                            id = 100;
                            foreach (string key in _vlans.Keys.Where(k => k.EndsWith(template.IsolationTag)))
                                id = Math.Max(id, _vlans[key]);
                            id += 1;
                            eth.Vlan = id;
                            _vlans.Add(eth.Net, id);
                        }

                    }
                }
            }
        }

        public string[] FindNetworks(string tag)
        {
            List<string> nets = new List<string>();
            nets.AddRange(_vlans.Keys.Where(x => !x.Contains("#")));
            nets.AddRange(_vlans.Keys.Where(x => x.Contains(tag)));
            nets.Sort();
            return nets.ToArray();
        }

    }
}
