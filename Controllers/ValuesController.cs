using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DrAppAPI.Models;
using System.Data.Entity;

namespace DrAppAPI.Controllers
{
    public class ValuesController : ApiController////a WebAPI ctrlr
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values/login = check credentials & return user_id
        /// <summary>
        /// For binding to form-data Http req MUST send either JSON str (& must specify Content-Type:application/json)--or--url-encoded (& specify Content-type:application/x-www-form-urlencoded)
        /// text/plain does NOT bind & can only accet data from query-str (not body/form data)
        /// </summary>
        /// <param name="loginPw"></param>
        /// <returns></returns>
        [Route("api/values/login")]
        public IHttpActionResult PostLogin([FromBody] UserBio userBio)//(string login, string pw) doesn't work coz [FromBody] can b applied to only 1 arg. Must have a complex type
        {
            using (ModelContainer db = new ModelContainer())
            {
                var user_id = db.Users.Where(x => x.loginName == userBio.loginName && x.pw == userBio.pw).Select(a => a.Id_User).FirstOrDefault();//.Any(x => x.loginName == loginPw.login && x.pw == loginPw.pw);

                if (user_id > 0)
                {
                    return Ok(user_id);//login success
                }
            }
            return Ok(0);//login fail
        }

        
        //POST - new user biodata
        [Route("api/values/newUser")]
        public IHttpActionResult PostNewUser([FromBody] UserBio u)
        {
            using (ModelContainer db = new ModelContainer())
            {
                if(db.Users.Any(x => x.loginName == u.loginName))//loginName already exists
                {
                    return Ok(0);
                }
                else
                {
                    User newUser = new DrAppAPI.User() { loginName=u.loginName, address=u.address, email=u.email, nameOfUser=u.nameOfUser, phone =u.phone, pw=u.pw };
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    return Ok(newUser.Id_User);//EF track newly inserted entity's auto-generated ID : https://stackoverflow.com/questions/5212751/how-can-i-get-id-of-inserted-entity-in-entity-framework
                }
            }
        }

        
        //POST - new appointment
        [Route("api/values/newAppointment")]
        public IHttpActionResult PostNewAppoint([FromBody] Appointment a)
        {
            Appointment newAppoint = new DrAppAPI.Appointment() { AppointmentTime = a.AppointmentTime, Clinic=a.Clinic, CreationTime=a.CreationTime, Doctor=a.Doctor, Id_User=a.Id_User};
            /*
            //chk if appoint time has another appoint within 30 min before it
            DateTime timeFrom, timeTo;
            timeTo = DateTime.Parse(appoint.AppointmentTime);
            timeFrom = timeTo.AddMinutes(-30);
            */
            using (var db = new ModelContainer())
            {
                db.Appointments.Add(newAppoint);
                db.SaveChanges();

                return Ok(newAppoint.Id_Appointment);
            }
        }

        //POST - All appoints
        [Route("api/values/Appointments/{user_id}")]
        public IHttpActionResult GetAllAppoints(int user_id)
        {
            using (var db = new ModelContainer())
            {
                /*Eager Loading : https://msdn.microsoft.com/en-us/library/jj574232(v=vs.113).aspx
                  3 types of loading asso tbl: {1}Eager .Include() {2}Lazy: just access the rel prop {3}Explicit: .Load()
                */

                /*when return Ok(List of all appointments) => following line is needed in Global.asax, or else err while converting list into JSON
                  //GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; : https://social.msdn.microsoft.com/Forums/vstudio/en-US/a5adf07b-e622-4a12-872d-40c753417645/web-api-error-the-objectcontent1-type-failed-to-serialize-the-response-body-for-content?forum=wcf
                  Alternately, don't return objs of edmx rather create proxy entities & return those!!! : https://stackoverflow.com/questions/23098191/failed-to-serialize-the-response-in-web-api-with-json?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
                */

                //.Include() reqd using System.Data.Entity;
                var allAppoints = db.Users.Where(x => x.Id_User == user_id).Include(u=>u.Appointments); //all appoints for a user
                return Ok(allAppoints.ToList());
            }
        }
        
        /*
        //POST - Edit Appoint
        [Route("api/values/UpdateAppoint")]
        public IHttpActionResult PutEditAppoint(int id, [FromBody] UserBio userBio)
        {
            return Ok(0);//login fail
        }
        */

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
