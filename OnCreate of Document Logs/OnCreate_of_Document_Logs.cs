﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace OnCreate_of_Document_Logs
{
    public class OnCreate_of_Document_Logs : IPlugin
    {
        public IPluginExecutionContext _context = null;
        public IOrganizationServiceFactory _serviceFactory = null;
        public IOrganizationService service = null;
        public ITracingService trace = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            _serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = _serviceFactory.CreateOrganizationService(_context.UserId);
            trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("Entered the plugin.");

            // ... switch on the MessageName
            //     and call actions to handle the supported messages
            switch (_context.MessageName.ToUpperInvariant())
            {
                case "CREATE":
                    OnCreate();
                    break;
                case "UPDATE":
                    OnUpdate();
                    break;
                //case "DELETE":
                //    OnDelete();
                //    break;
                default:
                    break;
            }
        }

        private void OnCreate()
        {
            trace.Trace("update");

            if (_context.InputParameters.Contains("Target") && _context.InputParameters["Target"] is Entity)
            {
                // Get Record
                Entity createdDocumentLog = (Entity)_context.InputParameters["Target"];


                if (createdDocumentLog.LogicalName == "arq_documentlog")
                {
                    // New Record
                    Entity createdDocumentPartyLogFROM = new Entity("arq_documentpartylog");
                    Entity createdDocumentPartyLogTO = new Entity("arq_documentpartylog");
                    String username = "";
                    var number = 0;
                    var stringnumber = "";
                    var prefixcode = "";
                    string currentyear = DateTime.Now.Year.ToString();
                    string last2digitsofyear = currentyear.Substring(currentyear.Length - 2);

                    EntityReference relatedEntityReference = createdDocumentLog.GetAttributeValue<EntityReference>("arq_doctype");
                    if (relatedEntityReference != null)
                    {
                        Guid doctypeid = relatedEntityReference.Id;
                        ColumnSet columns = new ColumnSet("arq_documentclassification");
                        Entity docRecord = service.Retrieve("arq_documenttype", doctypeid, columns);
                        OptionSetValue optionSetValue = docRecord.GetAttributeValue<OptionSetValue>("arq_documentclassification");
                        int doctype = optionSetValue.Value;


                        if (doctype == 1)
                        {
                            //  "Internal";
                            username = "Internal";
                            prefixcode = "INT" + last2digitsofyear + "-";


                        }
                        else if (doctype == 2)
                        {
                            // Inbound";
                            username = "External";
                            prefixcode = "INB" + last2digitsofyear + "-";

                        }
                        else if (doctype == 3)
                        {
                            // Outbound";
                            username = "External";
                            prefixcode = "OUT" + last2digitsofyear + "-";
                        }
                        else
                        {

                        }




                        createdDocumentPartyLogFROM["arq_document"] = new EntityReference("arq_documentlog", createdDocumentLog.Id);
                        createdDocumentPartyLogTO["arq_document"] = new EntityReference("arq_documentlog", createdDocumentLog.Id);

                    }

                    Guid newRecordIdFROM = service.Create(createdDocumentPartyLogFROM);
                    Guid newRecordIdTO = service.Create(createdDocumentPartyLogTO);


                    ColumnSet columns2 = new ColumnSet("arq_code");

                    Entity retrievedRecordfrom = service.Retrieve("arq_documentpartylog", newRecordIdFROM, columns2);
                    var codefrom = retrievedRecordfrom.GetAttributeValue<String>("arq_code");
                    var x = codefrom.Substring(6, codefrom.Length - 6);

                    retrievedRecordfrom["arq_name"] = $"DPL{last2digitsofyear}-{x}-From";
                    retrievedRecordfrom["arq_code"] = $"DPL{last2digitsofyear}-{x}";

                    service.Update(retrievedRecordfrom);


                    Entity retrievedRecordto = service.Retrieve("arq_documentpartylog", newRecordIdTO, columns2);
                    var codeto = retrievedRecordto.GetAttributeValue<String>("arq_code");
                    var y = codeto.Substring(6, codeto.Length - 6);

                    retrievedRecordto["arq_name"] = $"DPL{last2digitsofyear}-{y}-To";
                    retrievedRecordto["arq_code"] = $"DPL{last2digitsofyear}-{y}";


                    service.Update(retrievedRecordto);










                    createdDocumentPartyLogTO["arq_name"] = $"name";







                    Entity myentity = service.Retrieve(createdDocumentLog.LogicalName, createdDocumentLog.Id, new ColumnSet("arq_name"));



                    myentity["arq_docfromparty"] = new EntityReference("arq_documentlog", newRecordIdFROM);
                    myentity["arq_doctoparty"] = new EntityReference("arq_documentlog", newRecordIdTO);

                    var code = createdDocumentLog.GetAttributeValue<string>("arq_code").ToString();
                    myentity["arq_code"] = prefixcode + code;
                    myentity["arq_name"] = prefixcode + code + "-" + createdDocumentLog.GetAttributeValue<string>("arq_subject");
                    service.Update(myentity);


                    //trace.Trace("Novo registro criado na entidade destino com ID: " + newRecordId);
                }
            }
        }

        private void OnUpdate()
        {
            trace.Trace("update");
            var username = "";
            var number = 0;
            var stringnumber = "";
            var prefixcode = "";
            string currentyear = DateTime.Now.Year.ToString();
            string last2digitsofyear = currentyear.Substring(currentyear.Length - 2);


            if (_context.InputParameters.Contains("Target") && _context.InputParameters["Target"] is Entity)
            {

                trace.Trace("if");

                // Get Record

                Entity updatedDocumentLog = (Entity)_context.InputParameters["Target"];


                if (updatedDocumentLog.LogicalName == "arq_documentlog")
                {
                    //New Record
                    trace.Trace("1");





                    trace.Trace("2");


                    EntityReference relatedEntityReference = updatedDocumentLog.GetAttributeValue<EntityReference>("arq_doctype");


                    if (relatedEntityReference != null)
                    {
                        trace.Trace("3");
                        Guid doctypeid = relatedEntityReference.Id;
                        ColumnSet columns = new ColumnSet("arq_documentclassification");
                        Entity docRecord = service.Retrieve("arq_documenttype", doctypeid, columns);
                        OptionSetValue optionSetValue = docRecord.GetAttributeValue<OptionSetValue>("arq_documentclassification");
                        int doctype = optionSetValue.Value;
                        trace.Trace("4");


                        if (doctype == 1)
                        {
                            //  "Internal";
                            username = "Internal";
                            prefixcode = "INT" + last2digitsofyear + "-";


                        }
                        else if (doctype == 2)
                        {
                            // Inbound";
                            username = "External";
                            prefixcode = "INB" + last2digitsofyear + "-";

                        }
                        else if (doctype == 3)
                        {
                            // Outbound";
                            username = "External";
                            prefixcode = "OUT" + last2digitsofyear + "-";
                        }
                        else
                        {

                        }

                        Entity myentity = service.Retrieve(updatedDocumentLog.LogicalName, updatedDocumentLog.Id, new ColumnSet("arq_name"));

                        var prename = myentity.GetAttributeValue<string>("arq_name").ToString();

                        String[] vector = prename.Split('-');
                        Array.Reverse(vector);
                        Array.Resize(ref vector, vector.Length - 1);
                        Array.Reverse(vector);
                        string result = string.Join("-", vector);
                        myentity["arq_name"] = prefixcode + result;


                        Array.Resize(ref vector, vector.Length - 1);
                        string resultcode = string.Join("-", vector);

                        myentity["arq_code"] = prefixcode + resultcode;





                        service.Update(myentity);


                    }

                    trace.Trace("3");
                    String relatedEntityReferenceSubject = updatedDocumentLog.GetAttributeValue<String>("arq_subject");


                    Entity myentity = service.Retrieve(updatedDocumentLog.LogicalName, updatedDocumentLog.Id, new ColumnSet("arq_name", "arq_subject"));
                    var prename = myentity.GetAttributeValue<string>("arq_name");
                    String[] vector = prename.Split('-');

                    if (vector.Length == 3)
                    {
                        Array.Resize(ref vector, vector.Length - 1);
                    }

                    if (relatedEntityReferenceSubject == null || relatedEntityReferenceSubject == "")
                    {
                        trace.Trace("subject is null");
                        myentity["arq_name"] = string.Join("-", vector);
                    }
                    else
                    {
                        trace.Trace("subject");
                        var subj = myentity.GetAttributeValue<string>("arq_subject");
                        myentity["arq_name"] = string.Join("-", vector) + "-" + subj;
                    }

                    if (_context.Depth == 1)
                    {
                        service.Update(myentity);
                    }

                }


                trace.Trace("else");
            }





        }
    }
}
