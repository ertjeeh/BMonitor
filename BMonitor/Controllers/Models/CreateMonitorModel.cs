using BMonitor.DAL.Models.Monitors;
using Monitor = BMonitor.DAL.Models.Monitor;

namespace BMonitor.Controllers.Models;

public class CreateMonitorModel
{
    public string Name { get; set; }
    public string UpdateIntervalInMs { get; set; }

    public string MonitorType { get; set; }

    public string? PingEndpoint { get; set; }
    public string? PingTimeout { get; set; }

    public string? FolderPath { get; set; }

    public string? HttpEndpoint { get; set; }
    public string? HttpExpectedStatusCode { get; set; }
    public string? HttpTimeout { get; set; }

    public string? SqlConnStr { get; set; }
    public string? SqlQuery { get; set; }

    public string Validate()
    {
        var errorMessages = new List<string>();
        if (string.IsNullOrEmpty(MonitorType))
        {
            errorMessages.Add("MonitorType not provided. Please select a monitor type.");
        }

        if (string.IsNullOrEmpty(Name))
        {
            errorMessages.Add("Name needs to have a value.");
        }

        if (string.IsNullOrEmpty(UpdateIntervalInMs))
        {
            errorMessages.Add("UpdateIntervalInMs needs to have a value.");
        }
        else if (!int.TryParse(UpdateIntervalInMs, out int ui))
        {
            errorMessages.Add("UpdateIntervalInMs needs to have a number value.");
        }
        else if (ui > 2000)
        {
            errorMessages.Add("UpdateIntervalInMs needs a value <= 2000");
        }

        if (MonitorType == "mt_ping")
        {
            if (string.IsNullOrEmpty(PingEndpoint))
            {
                errorMessages.Add("PingEndpoint needs to have a value.");
            }

            if (string.IsNullOrEmpty(PingTimeout))
            {
                errorMessages.Add("PingTimeout needs to have a value.");
            }
            else if (!int.TryParse(PingTimeout, out int pt))
            {
                errorMessages.Add("PingTimeout needs to have a number value.");
            }
            else if (pt > 2000)
            {
                errorMessages.Add("PingTimeout needs a value <= 2000");
            }
        }

        if (MonitorType == "mt_folder")
        {
            if (string.IsNullOrEmpty(FolderPath))
            {
                errorMessages.Add("FolderPath needs to have a value.");
            }
        }

        if (MonitorType == "mt_http")
        {
            if (string.IsNullOrEmpty(HttpEndpoint))
            {
                errorMessages.Add("HttpEndpoint needs to have a value.");
            }

            if (string.IsNullOrEmpty(HttpExpectedStatusCode))
            {
                errorMessages.Add("HttpExpectedStatusCode needs to have a value.");
            }
            else if (!int.TryParse(HttpExpectedStatusCode, out int sc))
            {
                errorMessages.Add("HttpExpectedStatusCode needs to have a number value.");
            }
            else if (sc > 999)
            {
                errorMessages.Add("HttpExpectedStatusCode needs a value < 1000.");
            }

            if (string.IsNullOrEmpty(HttpTimeout))
            {
                errorMessages.Add("HttpTimeout needs to have a value.");
            }
            else if (!int.TryParse(HttpTimeout, out int ht))
            {
                errorMessages.Add("HttpTimeout needs to have a number value.");
            }
            else if (ht > 2000)
            {
                errorMessages.Add("HttpTimeout needs a value <= 2000");
            }
        }

        if (MonitorType == "mt_sql")
        {
            if (string.IsNullOrEmpty(SqlConnStr))
            {
                errorMessages.Add("SqlConnStr needs to have a value.");
            }

            if (string.IsNullOrEmpty(SqlQuery))
            {
                errorMessages.Add("SqlQuery needs to have a value.");
            }
        }

        return errorMessages.Count > 0
            ? string.Join(Environment.NewLine, errorMessages)
            : string.Empty;
    }

    internal Monitor? ToDbObject() => MonitorType != null
        ? MonitorType switch
        {
            "mt_ping" => new PingMonitor
            {
                Name = Name,
                UpdateIntervalInMs = Convert.ToInt32(UpdateIntervalInMs),
                Endpoint = PingEndpoint,
                TimeoutInMs = Convert.ToInt32(PingTimeout)
            },
            "mt_folder" => new FolderMonitor
            {
                Name = Name,
                UpdateIntervalInMs = Convert.ToInt32(UpdateIntervalInMs),
                Path = FolderPath
            },
            "mt_http" => new HttpMonitor
            {
                Name = Name,
                UpdateIntervalInMs = Convert.ToInt32(UpdateIntervalInMs),
                Endpoint = HttpEndpoint,
                ExpectedStatusCode = Convert.ToInt32(HttpExpectedStatusCode),
                TimeoutInMs = Convert.ToInt32(HttpTimeout)
            },
            "mt_sql" => new SqlMonitor
            {
                Name = Name,
                UpdateIntervalInMs = Convert.ToInt32(UpdateIntervalInMs),
                ConnectionString = SqlConnStr,
                Query = SqlQuery
            },
            _ => null
        }
    : null;
}