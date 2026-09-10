using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;
using Object = System.Object;

namespace NabaGame.Googlesheet.Importer.Editor
{
    public class GoogleSheetController
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private string ApplicationName = "GoogleSheet Reader";
        private string spreadsheetId;
        private SheetsService sheetService;

        public GoogleSheetController(string _spreadsheetId, string credentialFilePath)
        {
            spreadsheetId = _spreadsheetId;
            GoogleCredential credential;
            using (var stream =
                   new FileStream(credentialFilePath, FileMode.Open,
                       FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            sheetService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public List<string> GetAllSheetName()
        {
            List<string> ranges = new List<string>();
            bool includeGridData = false;

            SpreadsheetsResource.GetRequest request = sheetService.Spreadsheets.Get(spreadsheetId);
            request.Ranges = ranges;
            request.IncludeGridData = includeGridData;

            Spreadsheet response = request.Execute();
            if (response.Sheets != null)
            {
                List<string> sheetNameList = new List<string>();
                foreach (var sheet in response.Sheets)
                {
                    sheetNameList.Add(sheet.Properties.Title);
                }

                return sheetNameList;
            }

            return null;
        }

        public IList<IList<Object>> GetValueRange(string sheetName, string range)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                sheetService.Spreadsheets.Values.Get(spreadsheetId, $"{sheetName}!{range}");

            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                return values;
            }
            else
            {
                Console.WriteLine("No data found.");
                return null;
            }
        }

        public bool SetValueRange(string sheetName, IList<IList<object>> data)
        {
            ValueRange valueRange = new ValueRange { Values = data };
            string range = $"{sheetName}!A2:Z";

            SpreadsheetsResource.ValuesResource.UpdateRequest request =
                sheetService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            request.ValueInputOption =
                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            UpdateValuesResponse response = request.Execute();
            if (response.UpdatedCells == null)
            {
                return false;
            }
            else
            {
                Debug.Log($"Update Sheet OK : {response.UpdatedRange}");
                return true;
            }
        }

        public Dictionary<string, IList<IList<Object>>> GetAllSheetValueRange(List<string> sheetNames)
        {
            SpreadsheetsResource.ValuesResource.BatchGetRequest request =
                sheetService.Spreadsheets.Values.BatchGet(spreadsheetId);
            request.Ranges = sheetNames;
            BatchGetValuesResponse response = request.Execute();
            Dictionary<string, IList<IList<Object>>> sheetValueDict = new Dictionary<string, IList<IList<object>>>();
            for (int i = 0; i < response.ValueRanges.Count; i++)
            {
                sheetValueDict.Add(sheetNames[i], response.ValueRanges[i].Values);
            }

            return sheetValueDict;
        }
    }
}