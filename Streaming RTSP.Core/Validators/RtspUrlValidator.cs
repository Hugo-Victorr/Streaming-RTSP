using System;
using System.Text.RegularExpressions;

namespace Streaming_RTSP.Core.Validators
{
    /// <summary>
    /// Validador para URLs RTSP.
    /// Verifica se a URL segue o formato correto de uma URL RTSP.
    /// </summary>
    public static class RtspUrlValidator
    {
        /// <summary>
        /// Padrão de URL RTSP: rtsp://[host]:[port]/[path]
        /// - Aceita: rtsp://localhost:8554/stream
        /// - Aceita: rtsp://192.168.1.100:554/
        /// - Rejeita: http://localhost:8554/stream
        /// - Rejeita: rtsp://invalid
        /// </summary>
        private static readonly Regex RtspUrlPattern = new Regex(
            @"^rtsp://([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\:[0-9]+)?(/.*)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex IpAddressPattern = new Regex(
            @"^rtsp://(\d{1,3}\.){3}\d{1,3}(\:\d+)?(/.*)?$",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Valida se a URL é uma URL RTSP válida.
        /// </summary>
        /// <param name="url">A URL a ser validada</param>
        /// <param name="errorMessage">Mensagem de erro descritiva se inválida</param>
        /// <returns>True se válida, False caso contrário</returns>
        public static bool IsValid(string? url, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(url))
            {
                errorMessage = "A URL não pode estar vazia.";
                return false;
            }

            url = url.Trim();

            if (!url.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "A URL deve começar com 'rtsp://'.";
                return false;
            }

            if (url.Length < 9)
            {
                errorMessage = "A URL RTSP é muito curta. Formato esperado: rtsp://[host]:[porta]/[caminho]";
                return false;
            }

            if (url.Contains(" "))
            {
                errorMessage = "A URL não pode conter espaços em branco.";
                return false;
            }

            bool isValidDomain = RtspUrlPattern.IsMatch(url);
            bool isValidIp = IpAddressPattern.IsMatch(url);

            if (!isValidDomain && !isValidIp)
            {
                errorMessage = "Formato inválido. Use: rtsp://[host]:[porta]/[caminho]\n" +
                               "Exemplos:\n" +
                               "  • rtsp://localhost:8554/stream\n" +
                               "  • rtsp://192.168.1.100:554/\n" +
                               "  • rtsp://example.com/webcam";
                return false;
            }

            if (url.Contains(":"))
            {
                var hostWithPort = url.Substring(7);
                var parts = hostWithPort.Split('/');
                var hostPort = parts[0];

                if (hostPort.Contains(":"))
                {
                    var portStr = hostPort.Split(':')[1];
                    if (!int.TryParse(portStr, out int port) || port < 1 || port > 65535)
                    {
                        errorMessage = $"Porta inválida: {portStr}. A porta deve estar entre 1 e 65535.";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Valida se a URL é uma URL RTSP válida (versão simplificada).
        /// </summary>
        /// <param name="url">A URL a ser validada</param>
        /// <returns>True se válida, False caso contrário</returns>
        public static bool IsValid(string? url)
        {
            return IsValid(url, out _);
        }
    }
}
