using Hospital_Managment_System.Enums;
using Hospital_Managment_System.Models;

namespace Hospital_Managment_System.Helpers
{
    public static class LabTestHelper
    {
        public static string GetUnitsForTest(LabTestName testName)
        {
            return testName switch
            {
                LabTestName.CompleteBloodCount => "x10^9/L",
                LabTestName.BasicMetabolicPanel => "mg/dL",
                LabTestName.ComprehensiveMetabolicPanel => "mg/dL",
                LabTestName.LipidPanel => "mg/dL",
                LabTestName.LiverFunctionTest => "U/L",
                // Add other tests
                _ => "Units"
            };
        }

        public static string GetReferenceRangeForTest(LabTestName testName)
        {
            return testName switch
            {
                LabTestName.CompleteBloodCount => "4.0 - 11.0 x10^9/L",
                LabTestName.BasicMetabolicPanel => "70 - 100 mg/dL",
                LabTestName.ComprehensiveMetabolicPanel => "8.5 - 10.2 mg/dL",
                LabTestName.LipidPanel => "130 - 200 mg/dL",
                // Add other tests
                _ => "N/A"
            };
        }

        public static string GetFlagForTestResult(LabTest labTest)
        {
            if (!double.TryParse(labTest.TestResult, out var result))
            {
                return "N/A";
            }

            return labTest.TestName switch
            {
                LabTestName.CompleteBloodCount => result < 4.0 ? "LOW" : result > 11.0 ? "HIGH" : "NORMAL",
                LabTestName.BasicMetabolicPanel => result < 70 ? "LOW" : result > 100 ? "HIGH" : "NORMAL",
                LabTestName.ComprehensiveMetabolicPanel => result < 8.5 ? "LOW" : result > 10.2 ? "HIGH" : "NORMAL",
                LabTestName.LipidPanel => result < 130 ? "LOW" : result > 200 ? "HIGH" : "NORMAL",
                // Add other tests
                _ => "N/A"
            };
        }
    }
}
