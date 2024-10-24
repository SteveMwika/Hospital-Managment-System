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
                LabTestName.ThyroidFunctionTest => "mU/L",
                LabTestName.HemoglobinA1c => "%",
                LabTestName.Urinalysis => "pH, Specific Gravity",
                LabTestName.BloodGlucose => "mg/dL",
                LabTestName.ProthrombinTime => "seconds",
                LabTestName.DDimers => "mg/L",
                LabTestName.CReactiveProtein => "mg/L",
                LabTestName.RheumatoidFactor => "IU/mL",
                LabTestName.ErythrocyteSedimentationRate => "mm/hr",
                LabTestName.IronStudies => "mcg/dL",
                LabTestName.VitaminDTest => "ng/mL",
                LabTestName.VitaminB12Test => "pg/mL",
                LabTestName.ElectrolytePanel => "mEq/L",
                LabTestName.ArterialBloodGas => "mmHg",
                LabTestName.BloodUreaNitrogen => "mg/dL",
                _ => "Units",
            };
        }

        public static string GetReferenceRangeForTest(LabTestName testName)
        {
            return testName switch
            {
                LabTestName.CompleteBloodCount => "4.0 - 11.0 x10^9/L",
                LabTestName.BasicMetabolicPanel => "70 - 100 mg/dL",
                LabTestName.ComprehensiveMetabolicPanel => "8.5 - 10.2 mg/dL",
                LabTestName.LipidPanel => "130 - 200 mg/dL (Total Cholesterol)",
                LabTestName.LiverFunctionTest => "10 - 40 U/L",
                LabTestName.ThyroidFunctionTest => "0.4 - 4.0 mU/L",
                LabTestName.HemoglobinA1c => "<5.7%",
                LabTestName.Urinalysis => "pH: 4.5 - 8.0, SG: 1.005 - 1.030",
                LabTestName.BloodGlucose => "70 - 99 mg/dL",
                LabTestName.ProthrombinTime => "11 - 13.5 seconds",
                LabTestName.DDimers => "<0.50 mg/L",
                LabTestName.CReactiveProtein => "<10 mg/L",
                LabTestName.RheumatoidFactor => "<20 IU/mL",
                LabTestName.ErythrocyteSedimentationRate => "0 - 20 mm/hr",
                LabTestName.IronStudies => "60 - 170 mcg/dL",
                LabTestName.VitaminDTest => "30 - 100 ng/mL",
                LabTestName.VitaminB12Test => "200 - 900 pg/mL",
                LabTestName.ElectrolytePanel => "Sodium: 135 - 145 mEq/L, Potassium: 3.5 - 5.0 mEq/L",
                LabTestName.ArterialBloodGas => "pH: 7.35 - 7.45, pCO2: 35 - 45 mmHg",
                LabTestName.BloodUreaNitrogen => "7 - 20 mg/dL",
                _ => "N/A",
            };
        }

        public static string GetFlagForTestResult(LabTest labTest)
        {
            // Ensure that the TestResult is not null or empty
            if (string.IsNullOrWhiteSpace(labTest.TestResult))
            {
                return "N/A";  // Return "N/A" if the test result is missing
            }

            double result;

            // Attempt to parse the result as a double, trimming any extra spaces or characters
            if (!double.TryParse(labTest.TestResult.Trim(), out result))
            {
                return "N/A";  // Return "N/A" if the result can't be parsed
            }

            // Handle each LabTestName with appropriate result flagging based on the parsed value
            return labTest.TestName switch
            {
                LabTestName.CompleteBloodCount => result < 4.0 ? "LOW" : result > 11.0 ? "HIGH" : "NORMAL",
                LabTestName.BasicMetabolicPanel => result < 70 ? "LOW" : result > 100 ? "HIGH" : "NORMAL",
                LabTestName.ComprehensiveMetabolicPanel => result < 8.5 ? "LOW" : result > 10.2 ? "HIGH" : "NORMAL",
                LabTestName.LipidPanel => result < 130 ? "LOW" : result > 200 ? "HIGH" : "NORMAL",
                LabTestName.LiverFunctionTest => result < 10 ? "LOW" : result > 40 ? "HIGH" : "NORMAL",
                LabTestName.ThyroidFunctionTest => result < 0.4 ? "LOW" : result > 4.0 ? "HIGH" : "NORMAL",
                LabTestName.HemoglobinA1c => result >= 5.7 ? "HIGH" : "NORMAL",
                LabTestName.BloodGlucose => result < 70 ? "LOW" : result > 99 ? "HIGH" : "NORMAL",
                LabTestName.ProthrombinTime => result < 11 ? "LOW" : result > 13.5 ? "HIGH" : "NORMAL",
                LabTestName.DDimers => result >= 0.5 ? "HIGH" : "NORMAL",
                LabTestName.CReactiveProtein => result >= 10 ? "HIGH" : "NORMAL",
                LabTestName.RheumatoidFactor => result >= 20 ? "HIGH" : "NORMAL",
                LabTestName.ErythrocyteSedimentationRate => result > 20 ? "HIGH" : "NORMAL",
                LabTestName.IronStudies => result < 60 ? "LOW" : result > 170 ? "HIGH" : "NORMAL",
                LabTestName.VitaminDTest => result < 30 ? "LOW" : result > 100 ? "HIGH" : "NORMAL",
                LabTestName.VitaminB12Test => result < 200 ? "LOW" : result > 900 ? "HIGH" : "NORMAL",
                LabTestName.ElectrolytePanel => result < 135 ? "LOW" : result > 145 ? "HIGH" : "NORMAL",
                LabTestName.BloodUreaNitrogen => result < 7 ? "LOW" : result > 20 ? "HIGH" : "NORMAL",
                _ => "NORMAL",  // Default to "NORMAL" for any test that doesn't explicitly have a flag
            };
        }
    }
}
