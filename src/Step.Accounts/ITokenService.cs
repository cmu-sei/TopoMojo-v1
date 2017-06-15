namespace Step.Accounts
{
    public interface ITokenService
    {
        object GenerateJwt(string guid);
    }
}
