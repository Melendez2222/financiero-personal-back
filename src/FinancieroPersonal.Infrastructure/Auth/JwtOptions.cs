namespace FinancieroPersonal.Infrastructure.Auth;

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "FinancieroPersonal";
    public string Audience { get; set; } = "FinancieroPersonal";
    public int ExpiraMinutos { get; set; } = 1440;
}
