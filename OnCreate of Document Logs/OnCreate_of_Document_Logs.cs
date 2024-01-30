using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
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
                //case "UPDATE":
                //    OnUpdate();
                //    break;
                //case "DELETE":
                //    OnDelete();
                //    break;
                default:
                    break;
            }
        }

        private void OnCreate()
        {
            // Verificando se o plugin está sendo acionado na criação de um registro
            if (_context.InputParameters.Contains("Target") && _context.InputParameters["Target"] is Entity)
            {
                // Obtendo o registro criado
                Entity createdEntity = (Entity)_context.InputParameters["Target"];

                // Verificando se o registro foi criado na entidade desejada
                if (createdEntity.LogicalName == "arq_documentlog")
                {
                    // Criando um novo registro na entidade destino
                    Entity newEntity = new Entity("arq_documentpartylog");
                    newEntity["arq_name"] = "Document Party Log:" + createdEntity.GetAttributeValue<string>("arq_subject");
                    Guid newRecordId = service.Create(newEntity);

                    // Definindo os valores do campo de lookup para associar os registros
                    createdEntity["arq_docfromparty"] = new EntityReference("arq_documentlog", newRecordId);
                    createdEntity["arq_doctoparty"] = new EntityReference("arq_documentlog", newRecordId);

                    service.Update(createdEntity);


                    trace.Trace("Novo registro criado na entidade destino com ID: " + newRecordId);
                }
            }
        }



        //private void OnUpdate()
        //{
        //    //trace.Trace("Entered the Update function.");
        //    if (_context.InputParameters.Contains("Target") && _context.InputParameters["Target"] is Entity)
        //    {
        //        //validations required to end loop
        //        //booking can't be status: Travel Time or Canceled
        //        Entity myEntity = (Entity)_context.InputParameters["Target"];
        //        Entity booking = service.Retrieve("bookableresourcebooking", myEntity.Id, new ColumnSet("bookingstatus", "ipg_originalstarttime", "ipg_traveltime", "starttime"));

        //        DateTime startTime = new DateTime();
        //        DateTime originalStartTime = new DateTime();
        //        int travelTime = 0;

        //        //startime value validation
        //        if (myEntity.Contains("starttime"))
        //        {
        //            startTime = myEntity.GetAttributeValue<DateTime>("starttime");
        //            //trace.Trace("Trigger contains starttime: " + startTime.ToString());
        //        }
        //        else
        //        {
        //            startTime = booking.GetAttributeValue<DateTime>("starttime");
        //            //trace.Trace("Trigger does not contain starttime. Getting from record: " + startTime.ToString());
        //        }

        //        //traveltime value validation
        //        if (myEntity.Contains("ipg_traveltime"))
        //        {
        //            travelTime = myEntity.GetAttributeValue<int>("ipg_traveltime");
        //            //trace.Trace("Trigger contains ipg_traveltime: " + travelTime.ToString());
        //        }
        //        else
        //        {
        //            travelTime = booking.GetAttributeValue<int>("ipg_traveltime");
        //            //trace.Trace("Trigger does not contain ipg_traveltime. Getting from record: " + travelTime.ToString());
        //        }

        //        //originalStartTime value validation
        //        if (booking.Contains("ipg_originalstarttime"))
        //        {
        //            originalStartTime = booking.GetAttributeValue<DateTime>("ipg_originalstarttime");
        //            //trace.Trace("Original Start Time: " + originalStartTime);
        //        }
        //        else
        //        {
        //            //trace.Trace("ipg_originalstarttime is null on the record.");
        //        }
        //        //check BookingStatus
        //        if (booking.GetAttributeValue<EntityReference>("bookingstatus").Name != "Travel Time" //Travel Time
        //            && booking.GetAttributeValue<EntityReference>("bookingstatus").Name != "Canceled") //Canceled  
        //        {
        //            //trace.Trace("Booking not Travel Time or Canceled, current startTime: " + startTime.ToString());

        //            //calculations verification
        //            //trace.Trace("Subtraction between start and travel: " + startTime.AddMinutes(-travelTime).ToString());

        //            //originalstartttime null?
        //            if (originalStartTime != startTime.AddMinutes(-travelTime))
        //            {
        //                //trace.Trace("All Validations sucessful!");

        //                //check information received in the trigger
        //                if (myEntity.Contains("starttime") && myEntity.Contains("ipg_traveltime"))
        //                {
        //                    //trace.Trace("Trigger contained starttime: " + startTime.ToString() + " and travelTime: " + travelTime.ToString());
        //                }
        //                else if (myEntity.Contains("ipg_traveltime"))
        //                {
        //                    startTime = originalStartTime;
        //                    //trace.Trace("Trigger contains ipg_traveltime: " + travelTime.ToString());
        //                }
        //                UpdateBookingOrTravel(myEntity, startTime, travelTime);
        //            }
        //            else
        //            {
        //                //trace.Trace("There is no difference between Original Start Time and the subtraction between start and travel.");
        //            }
        //        }
        //        else
        //        {
        //            //trace.Trace("Booking was Travel Time or Canceled.");
        //        }
        //    }
        //}

        //private void OnDelete()
        //{
        //    //trace.Trace("Entered the Delete function."); 
        //    var image = _context.PreEntityImages.FirstOrDefault().Value;
        //    //trace.Trace("Obtained the image: " + image.Id.ToString());
        //    if (image.Contains("ipg_relatedbooking"))
        //    {
        //        EntityReference relatedBooking = image.GetAttributeValue<EntityReference>("ipg_relatedbooking");
        //        onDeleteTravelResetBooking(relatedBooking);
        //    }
        //    else
        //    {
        //        //trace.Trace("Related Booking was empty");

        //        //not needed currently, as there is already a PA flow doing this.
        //        //get all related child bookings, and also delete
        //        //EntityCollection RelatedBookings = GetRelatedBookings(image.Id);
        //        //trace.Trace("Related Booking obtained: " + RelatedBookings.Entities.Count().ToString());
        //        //if (RelatedBookings.Entities.Count() > 0)
        //        //{
        //        //    foreach (Entity x in RelatedBookings.Entities)
        //        //    {
        //        //        trace.Trace("Related Booking to Delete: " + x.Id.ToString());
        //        //        service.Delete(x.LogicalName, x.Id);
        //        //    }
        //        //}
        //    }
        //}

        //private void onCreateSetOriginal(Entity trigger)
        //{
        //    //trace.Trace("Entered the onCreateSetOriginal function.");

        //    Entity myBookingRecord = service.Retrieve("bookableresourcebooking", trigger.Id, new ColumnSet("starttime"));
        //    DateTime startTime = myBookingRecord.GetAttributeValue<DateTime>("starttime");
        //    myBookingRecord["ipg_originalstarttime"] = startTime;
        //    service.Update(myBookingRecord);
        //    //trace.Trace("Booking record updated.");
        //}

        

    }
}
