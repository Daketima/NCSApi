using DataLayer.Data;
using DataLayer.DataContext;
using Microsoft.EntityFrameworkCore;
using NCS.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Console;

namespace CustomDutyJob
{
    class Program
    {

        static void Main(string[] args)
        {
            CustomContext _appContext = new CustomContext();
            string[] tossesFile = Directory.GetFiles(@"C:\tosser\inout\callback");

            if (tossesFile.Any())
            {
                foreach (var filename in tossesFile)
                {          
                    string rawXml = File.ReadAllText(filename);           

                    string _whatToOppress = rawXml.Contains("eExciseAssessmentNotice") ? ProcessExcise(filename) : rawXml.Contains("eAssessmentNotice") ? ProcessSDG(filename) : ProcessSD(filename);
                }
                WriteLine("Empting Callback folder, please wait ...");
                
                Thread.Sleep(5);
                cleaner.DeleteFile(@"C:\tosser\inout\callback");

                WriteLine("Callback Folder emptied");
            }
            ReadLine();
        }

        static string ProcessExcise(string filename)
        {            
            WriteLine($"Reading file...{filename}");
            string rawXml = File.ReadAllText(filename);

            XmlDocument xmlDoc = new XmlDocument();

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            xmlDoc.Load(fs);

            var xmlTaxesNodeList = xmlDoc.GetElementsByTagName("Taxes");

            var ser = new XmlSerializer(typeof(ExciseAssessment));
            //Real xml stream
            using (TextReader reader = new StringReader(xmlDoc.InnerXml))
            {
                // Serialize the xml document

                ExciseAssessment newAss = (ExciseAssessment)ser.Deserialize(reader);

                string serr = JsonConvert.SerializeObject(newAss);
                //AssessmentNotification newAss2 

                Assessment newAssNotify = JsonConvert.DeserializeObject<Assessment>(serr);

                using (CustomContext _context = new CustomContext())
                {
                    newAssNotify.AssessmentTypeId = (int)AssessmentType.Excise;
                    newAssNotify.DateCreated = DateTime.Now;
                    _context.Assessment.Add(newAssNotify);
                    _context.SaveChanges();
                }

                fs.Close();
                fs.Dispose();

                SaveTaxes(rawXml, newAssNotify.Id, "eExciseAssessmentNotice");                
                //SaveToArchive(@"C:\tosser\inout\archive\Excise\Assessment");
                var startIndex = filename.LastIndexOf('\\');
                var fileNameCopy = filename.Substring(startIndex + 1);
                CopyTo(filename, $"C:\\tosser\\inout\\archive\\Excise\\Assessment\\{fileNameCopy}");
                SaveRawXML(newAssNotify.Id, rawXml, $"C:\\tosser\\inout\\archive\\Excise\\Assessment\\{fileNameCopy}");
            }           
           
            return "Excise Assesment Processed";
        }

        enum AssessmentType
        {
            Excise = 1,
            SD = 2,
            SGD = 3
        }

        static string ProcessSDG(string filename)
        {
            WriteLine($"Reading file...{filename}");
            string rawXml = File.ReadAllText(filename);

            XmlDocument xmlDoc = new XmlDocument();

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            xmlDoc.Load(fs);

            //xmlDoc.LoadXml(RawXML);

            var xmlTaxesNodeList = xmlDoc.GetElementsByTagName("Taxes");

            var ser = new XmlSerializer(typeof(eAssessmentNotice));
            //Real xml stream
            using (TextReader reader = new StringReader(xmlDoc.InnerXml))
            {
                // Serialize the xml document

                eAssessmentNotice newAss = (eAssessmentNotice)ser.Deserialize(reader);

                string serr = JsonConvert.SerializeObject(newAss);
                //AssessmentNotification newAss2 

                Assessment newAssNotify = JsonConvert.DeserializeObject<Assessment>(serr);

                using (CustomContext _contexy = new CustomContext())
                {
                    newAssNotify.AssessmentTypeId = (int)AssessmentType.SGD;
                    newAssNotify.DateCreated = DateTime.Now;
                    _contexy.Assessment.Add(newAssNotify);
                    _contexy.SaveChanges();
                }

                fs.Close();
                fs.Dispose();

                SaveTaxes(rawXml, newAssNotify.Id, "eAssessmentNotice");
               
                // SaveToArchive(@"C:\tosser\inout\archive\SGD\Assessment");
                var startIndex = filename.LastIndexOf('\\');
                var fileNameCopy = filename.Substring(startIndex + 1);
                CopyTo(filename, $"C:\\tosser\\inout\\archive\\SGD\\Assessment\\{fileNameCopy}"); 
                SaveRawXML(newAssNotify.Id, rawXml, $"C:\\tosser\\inout\\archive\\SGD\\Assessment\\{fileNameCopy}");

            }
           
            //cleaner.DeleteFile(filename);

            return "SGD Assessment processes";
        }

        static string ProcessSD(string filename)
        {


            WriteLine($"Reading file...{filename}");
            string rawXml = File.ReadAllText(filename);

            XmlDocument xmlDoc = new XmlDocument();

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            xmlDoc.Load(fs);

            var xmlTaxesNodeList = xmlDoc.GetElementsByTagName("Taxes");

            var ser = new XmlSerializer(typeof(SDAssessment));
            //Real xml stream
            using (TextReader reader = new StringReader(xmlDoc.InnerXml))
            {
                // Serialize the xml document

                SDAssessment newAss = (SDAssessment)ser.Deserialize(reader);

                string serr = JsonConvert.SerializeObject(newAss);
                //AssessmentNotification newAss2 

                Assessment newAssNotify = JsonConvert.DeserializeObject<Assessment>(serr);

                using (CustomContext _context = new CustomContext())
                {
                    newAssNotify.AssessmentTypeId = (int)AssessmentType.SD;
                    newAssNotify.DateCreated = DateTime.Now;
                    _context.Assessment.Add(newAssNotify);
                    _context.SaveChanges();
                }

                fs.Close();
                fs.Dispose();

                SaveTaxes(rawXml, newAssNotify.Id, "sdAssessmentNotice");
               
                // SaveToArchive(@"C:\tosser\inout\archive\SD\Assessment");
                var startIndex = filename.LastIndexOf('\\');
                var fileNameCopy = filename.Substring(startIndex + 1);
                CopyTo(filename, $"C:\\tosser\\inout\\archive\\SD\\Assessment\\{fileNameCopy}");
                SaveRawXML(newAssNotify.Id, rawXml, $"C:\\tosser\\inout\\archive\\SD\\Assessment\\{fileNameCopy}");
            }          
            // cleaner.DeleteFile(filename);

            return "SD Assesment Processed";
        }

        //static void SaveToArchive(string FilePath)
        //{
        //    XmlDocument xmlDoc = new XmlDocument();

        //    FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
        //    xmlDoc.Load(fs);

        //    string path = System.IO.Path.GetDirectoryName(@"C:\tosser\inout\archive\Callback");
        //    string file = System.IO.Path.Combine(path, xmlDoc.InnerXml);

        //    fs.Close();
        //    fs.Dispose();

        //    using (TextReader reader = new StringReader(xmlDoc.InnerXml))
        //    {

        //        string targetFile = Path.Combine(Path.GetFileName(FilePath));
        //        //if (File.Exists(targetFile)) File.Delete(targetFile);
        //        // File.Copy(file, targetFile);
        //        // doc.Load(testdata);
        //        // xmldoc.Save(path);

        //        string propertyFile = @"C:\tosser\inout\archive\Callback\setting.xml";
        //        string propertyFolder = propertyFile.Substring(0, propertyFile.LastIndexOf("\\") + 1);
        //        string newXML = propertyFolder + targetFile;

        //        //XmlDocument doc name of xml document in code
        //        xmlDoc.Save(newXML);           

        //    }
            
        //}

       static void CopyTo(string ThisFile, string ToDestination) {
            File.Copy(ThisFile, ToDestination);
        }

        static void SaveTaxes(string rawXML, Guid AssessmentId, string xmlRoot)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(rawXML);

            var taxes = xmlDoc.SelectNodes(xmlRoot + "/Taxes/Tax");

            using (CustomContext cust = new CustomContext())
            {
                foreach (XmlNode node in taxes)
                {
                    Tax newTax = new Tax { AssessmentId = AssessmentId, TaxAmount = node["TaxAmount"].InnerText, TaxCode = node["TaxCode"].InnerText, DateCreated = DateTime.Now };
                    cust.Tax.Add(newTax);
                    cust.SaveChanges();
                }
            }
        }

        static void SaveRawXML(Guid assessmentId, string rawXml, string path)
        {
            using (CustomContext context = new CustomContext())
            {
                XMLArchive newArch = new XMLArchive { AssessmentId = assessmentId, Path = path, RawXML = rawXml, DateCreated = DateTime.Now };
                context.Add(newArch);
                context.SaveChanges();
            }
        }
    }

    class cleaner
    {
        public static void DeleteFile(string folderPath)
        {
            DirectoryInfo all = new DirectoryInfo(folderPath);

            foreach (FileInfo file in all.GetFiles())
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                file.IsReadOnly = false;
                file.Delete();
            }

            //    all.Delete(true);
            //    string pathString2 = @"C:\tosser\inout\callback";
            //System.IO.Directory.CreateDirectory(pathString2);

        }
    }

}

