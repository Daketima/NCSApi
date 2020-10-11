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

            Thread excise = new Thread(new ThreadStart(ProcessExcise));
            Thread sd = new Thread(new ThreadStart(ProcessSD));
            excise.Start();
            sd.Start();

            if (tossesFile.Any())
            {
                foreach (var filename in tossesFile)
                {

                    WriteLine($"Reading file...{filename}");
                    string rawXml = File.ReadAllText(filename);

                    XmlDocument xmlDoc = new XmlDocument();

                    FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    xmlDoc.Load(fs);

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

                        newAssNotify.AssessmentTypeId = (int)AssessmentType.SGD;
                        _appContext.Assessment.Add(newAssNotify);
                        _appContext.SaveChanges();

                        SaveTaxes(rawXml, newAssNotify.Id, "eAssessmentNotice");
                        SaveRawXML(newAssNotify.Id, rawXml, filename);
                        SaveToArchive(filename);
                    }
                    fs.Close();
                    fs.Dispose();
                    cleaner.DeleteFile(@"C:\tosser\inout\callback");                   
                }
            }

            //System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();
            //Thread.Sleep(1000); 
            
            excise.Join();
            sd.Join();
           
            ReadLine();
        }


        static void ProcessExcise()
        {
            CustomContext _appContext = new CustomContext();
            string[] tossesFile = Directory.GetFiles(@"C:\tosser\inout\excise");

            if (tossesFile.Any())
            {
                foreach (var filename in tossesFile)
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
                            _context.Assessment.Add(newAssNotify);
                            _context.SaveChanges();
                        }

                        SaveTaxes(rawXml, newAssNotify.Id, "eExciseAssessmentNotice");
                        SaveRawXML(newAssNotify.Id, rawXml, filename);
                        SaveToArchive(filename);
                    }
                    fs.Close();
                    fs.Dispose();
                    cleaner.DeleteFile(@"C:\tosser\inout\excise");
                }
            }
        }

        enum AssessmentType
        {
            Excise = 1,
            SD = 2,
            SGD = 3
        }

        static void ProcessSD()
        {

          //  CustomContext _appContext = new CustomContext();
            string[] tossesFile = Directory.GetFiles(@"C:\tosser\inout\sd");

            if (tossesFile.Any())
            {
                foreach (var filename in tossesFile)
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

                        using (CustomContext _contexy = new CustomContext())
                        {
                            newAssNotify.AssessmentTypeId = (int)AssessmentType.SD;
                            _contexy.Assessment.Add(newAssNotify);
                            _contexy.SaveChanges();
                        }

                        SaveTaxes(rawXml, newAssNotify.Id, "sdAssessmentNotice");
                        SaveRawXML(newAssNotify.Id, rawXml, filename);
                        SaveToArchive(filename);
                    }
                    fs.Close();
                    fs.Dispose();
                    cleaner.DeleteFile(@"C:\tosser\inout\sd");
                }
            }
        }


        static void SaveToArchive(string FilePath)
        {
            XmlDocument xmlDoc = new XmlDocument();

            FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            xmlDoc.Load(fs);

            string path = System.IO.Path.GetDirectoryName(@"C:\tosser\inout\archive\Callback");
            string file = System.IO.Path.Combine(path, xmlDoc.InnerXml);

            using (TextReader reader = new StringReader(xmlDoc.InnerXml))
            {

                string targetFile = Path.Combine(Path.GetFileName(FilePath));
                //if (File.Exists(targetFile)) File.Delete(targetFile);
                // File.Copy(file, targetFile);
                // doc.Load(testdata);
                // xmldoc.Save(path);

                string propertyFile = @"C:\tosser\inout\archive\Callback\setting.xml";
                string propertyFolder = propertyFile.Substring(0, propertyFile.LastIndexOf("\\") + 1);
                string newXML = propertyFolder + targetFile;

                //XmlDocument doc name of xml document in code
                xmlDoc.Save(newXML);

                fs.Close();
                fs.Dispose();

            }
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
                    Tax newTax = new Tax { AssessmentId = AssessmentId, TaxAmount = node["TaxAmount"].InnerText, TaxCode = node["TaxCode"].InnerText };
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
            //System.IO.Directory.CreateDirectory(pathString2) ;

        }
    }

}

