using System;
using System.Globalization;
using EventStreamSharp.Domain;

namespace EventStreamSharp.Ingest;

public class EventParser
{
    public EventRecord? Parse(string linha)
    {
        if (string.IsNullOrWhiteSpace(linha))
            return null;
        linha = linha.Replace("\uFEFF", "");
        var partes = linha
            .Split(',')
            .Select(x => x.Trim().Trim('\0').Trim('\r').Trim('\n'))
            .ToArray();

        if (partes.Length != 6)
            return null;

        // Timestamp robusto
        if (!DateTime.TryParse(partes[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var timestamp))
            return null;

        var serviceName = partes[1];
        var action = partes[2];

        // Parse de inteiros seguro
        if (!int.TryParse(partes[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var durationMs))
            return null;

        if (!int.TryParse(partes[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var payloadSize))
            return null;

        // Bool robusto
        var rawBool = partes[5].ToLowerInvariant();
        bool success = rawBool switch
        {
            "true" => true,
            "false" => false,
            _ => false
        };

        return new EventRecord(timestamp, serviceName, action, durationMs, payloadSize, success);
    }
}
