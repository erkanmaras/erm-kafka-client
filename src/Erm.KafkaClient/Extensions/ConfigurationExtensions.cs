using Confluent.Kafka;
using Erm.KafkaClient.Configuration;

namespace Erm.KafkaClient;

internal static class ConfigurationExtensions
{

    public static void ReadSecurityInformation(this ClientConfig config, ClusterConfiguration cluster)
    {
        var securityInformation = cluster.GetSecurityInformation();

        if (securityInformation is null) return;

        config.SecurityProtocol = securityInformation.SecurityProtocol;
        config.SslCaLocation = securityInformation.SslCaLocation;
        config.SslCertificateLocation = securityInformation.SslCertificateLocation;
        config.SslCertificatePem = securityInformation.SslCertificatePem;
        config.SslCipherSuites = securityInformation.SslCipherSuites;
        config.SslCrlLocation = securityInformation.SslCrlLocation;
        config.SslCurvesList = securityInformation.SslCurvesList;
        config.SslKeyLocation = securityInformation.SslKeyLocation;
        config.SslKeyPassword = securityInformation.SslKeyPassword;
        config.SslKeyPem = securityInformation.SslKeyPem;
        config.SslKeystoreLocation = securityInformation.SslKeystoreLocation;
        config.SslKeystorePassword = securityInformation.SslKeystorePassword;
        config.SslSigalgsList = securityInformation.SslSigalgsList;
        config.SslEndpointIdentificationAlgorithm = securityInformation.SslEndpointIdentificationAlgorithm;
        config.EnableSslCertificateVerification = securityInformation.EnableSslCertificateVerification;
        config.SaslKerberosKeytab = securityInformation.SaslKerberosKeytab;
        config.SaslKerberosPrincipal = securityInformation.SaslKerberosPrincipal;
        config.SaslOauthbearerConfig = securityInformation.SaslOauthbearerConfig;
        config.SaslKerberosKinitCmd = securityInformation.SaslKerberosKinitCmd;
        config.SaslKerberosServiceName = securityInformation.SaslKerberosServiceName;
        config.SaslKerberosMinTimeBeforeRelogin = securityInformation.SaslKerberosMinTimeBeforeRelogin;
        config.EnableSaslOauthbearerUnsecureJwt = securityInformation.EnableSaslOauthbearerUnsecureJwt;
        config.SaslMechanism = securityInformation.SaslMechanism;
        config.SaslUsername = securityInformation.SaslUsername;
        config.SaslPassword = securityInformation.SaslPassword;
    }
}