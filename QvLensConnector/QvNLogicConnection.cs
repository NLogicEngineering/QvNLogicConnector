using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QlikView.Qvx.QvxLibrary;
using QvNLogicConnector.NLogic;

namespace QvNLogicConnector
{
    internal class QvNLogicConnection : QvxConnection
    {
        private string query;

        public override void Init()
        {
            QvxLog.SetLogLevels(true, true);
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Init()");

            var programFields = new[] {
                new QvxField(Resources.Audience, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                new QvxField(Resources.Market, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                new QvxField(Resources.ProgramName, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                new QvxField(Resources.Daypart, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                new QvxField(Resources.StationName, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                new QvxField(Resources.FirstAiring, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.DATE),
                new QvxField(Resources.LastAiring, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.DATE),
                new QvxField(Resources.Weekdays, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                new QvxField(Resources.StartTime, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                new QvxField(Resources.EndTime, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                new QvxField(Resources.Duration, QvxFieldType.QVX_SIGNED_INTEGER, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.INTEGER),
                new QvxField(Resources.NumAired, QvxFieldType.QVX_SIGNED_INTEGER, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.INTEGER),
                new QvxField(Resources.AMA, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.Rating, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.Share, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.CumeReach, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.CumeReachPercent, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.DailyReach, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.DailyReachPercent, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.WeeklyReach, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.WeeklyReachPercent, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.Frequency, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.Impressions, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
                new QvxField(Resources.ReachPercent, QvxFieldType.QVX_IEEE_REAL, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.REAL),
            };

            var MTable = new QvxTable {
                TableName = "ReportResults",
                GetRows = GetProgramRows,
                Fields = programFields
            };

            MTables = new List<QvxTable> {
                MTable
            };
        }

        public override QvxDataTable ExtractQuery(string query, List<QvxTable> tables)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, $"ExtractQuery: {query}");

            this.query = query;

            return new QvxDataTable(FindTable("ReportResults", tables));
        }

        private IEnumerable<QvxDataRow> GetProgramRows()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, $"GetProgramRows");

            ReportResponse reportResponse;
            QvxTable table;
            try
            {
                string username, password;
                if (!this.MParameters.TryGetValue("UserId", out username) ||
                    !this.MParameters.TryGetValue("Password", out password))
                    throw new AuthenticationException("Username and/or password is incorrect");

                var arguments = JsonConvert.DeserializeObject<ReportRequest>(query);
                var client = new AnalyticsServiceClient();
                client.ClientCredentials.UserName.UserName = username;
                client.ClientCredentials.UserName.Password = password;

                reportResponse = client.GetReport(arguments);
                if (reportResponse.Status == ResultStatus.Error)
                {
                    var message = string.Join(", ", reportResponse.Messages);
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, message);
                    throw new Exception("Server returned error: " + message);
                }

                table = FindTable("ReportResults", MTables);
            }
            catch (Exception ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, $"Exception: {ex.Message}");
                throw;
            }

            var audienceField = table[Resources.Audience];
            var marketField = table[Resources.Market];
            var progrmaNameField = table[Resources.ProgramName];
            var daypartField = table[Resources.Daypart];
            var stationNameField = table[Resources.StationName];
            var firstAiringField = table[Resources.FirstAiring];
            var lastAiringField = table[Resources.LastAiring];
            var weekdayField = table[Resources.Weekdays];
            var startTimeField = table[Resources.StartTime];
            var endTimeField = table[Resources.EndTime];
            var durationField = table[Resources.Duration];
            var numAiredField = table[Resources.NumAired];
            var amaField = table[Resources.AMA];
            var ratingField = table[Resources.Rating];
            var shareField = table[Resources.Share];
            var cumeReachField = table[Resources.CumeReach];
            var cumeReachPercentField = table[Resources.CumeReachPercent];
            var dailyReachField = table[Resources.DailyReach];
            var dailyReachPercentField = table[Resources.DailyReachPercent];
            var weeklyReachField = table[Resources.WeeklyReach];
            var weeklyReachPercentField = table[Resources.WeeklyReachPercent];
            var frequencyField = table[Resources.Frequency];
            var impressionsField = table[Resources.Impressions];
            var reachPercentField = table[Resources.ReachPercent];

            foreach (var row in reportResponse.Results)
            {
                var dataRow = new QvxDataRow();
                for (int audienceIndex = 0; audienceIndex < reportResponse.AudienceInformation.Length; audienceIndex++)
                {
                    for (int marketIndex = 0; marketIndex < reportResponse.Markets.Length; marketIndex++)
                    {
                        var rowValues = row.StatsPerAudience[audienceIndex].PerMarket[marketIndex];

                        dataRow[audienceField] = reportResponse.AudienceInformation[audienceIndex].Label ?? "";
                        dataRow[marketField] = reportResponse.Markets[marketIndex].Name ?? "";
                        if (row.ProgramName != null) dataRow[progrmaNameField] = row.ProgramName;
                        if (row.DaypartLabel != null) dataRow[daypartField] = row.DaypartLabel;
                        if (row.StationName != null) dataRow[stationNameField] = row.StationName;
                        if (row.FirstAirDate.HasValue) dataRow[firstAiringField] = row.FirstAirDate.Value.ToString("yyyy-MM-dd");
                        if (row.LastAirDate.HasValue) dataRow[lastAiringField] = row.LastAirDate.Value.ToString("yyyy-MM-dd");
                        if (row.Weekdays != null) dataRow[weekdayField] = row.Weekdays;
                        if (row.StartTime != null) dataRow[startTimeField] = row.StartTime;
                        if (row.EndTime != null) dataRow[endTimeField] = row.EndTime;
                        if (row.Duration.HasValue) dataRow[durationField] = row.Duration.Value;
                        if (row.NumAired.HasValue) dataRow[numAiredField] = row.NumAired.Value;
                        if (rowValues.AMA.HasValue) dataRow[amaField] = rowValues.AMA.Value;
                        if (rowValues.Rating.HasValue) dataRow[ratingField] = rowValues.Rating.Value;
                        if (rowValues.SharePercent.HasValue) dataRow[shareField] = rowValues.SharePercent.Value;
                        if (rowValues.CumulativeReach.HasValue) dataRow[cumeReachField] = rowValues.CumulativeReach.Value;
                        if (rowValues.CumulativeReachPercent.HasValue) dataRow[cumeReachPercentField] = rowValues.CumulativeReachPercent.Value;
                        if (rowValues.DailyReach.HasValue) dataRow[dailyReachField] = rowValues.DailyReach.Value;
                        if (rowValues.DailyReachPercent.HasValue) dataRow[dailyReachPercentField] = rowValues.DailyReachPercent.Value;
                        if (rowValues.WeeklyReach.HasValue) dataRow[weeklyReachField] = rowValues.WeeklyReach.Value;
                        if (rowValues.WeeklyReachPercent.HasValue) dataRow[weeklyReachPercentField] = rowValues.WeeklyReachPercent.Value;
                        if (rowValues.EstimatedAverageFrequency.HasValue) dataRow[frequencyField] = rowValues.EstimatedAverageFrequency.Value;
                        if (rowValues.EstimatedImpressions.HasValue) dataRow[impressionsField] = rowValues.EstimatedImpressions.Value;
                        if (rowValues.EstimatedReachPercent.HasValue) dataRow[reachPercentField] = rowValues.EstimatedReachPercent.Value;
                    }
                }

                yield return dataRow;
            }
        }
    }
}
