using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices.ActiveDirectory;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Dynamic;
using IMS.Classess;
using IMS.Models;
using System.Web.Hosting;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.AccessControl;
using System.DirectoryServices;
using System.Net.Mail;
using spms_ws.Models;
using System.Net;
using System.Collections.Specialized;
using PphisWebService.Models;
using System.Drawing.Drawing2D;

namespace spms_ws
{
    /// <summary>
    /// Summary description for sync_adapter
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class sync_adapter : System.Web.Services.WebService
    {
        public string networkPath = @"\\192.168.2.210\pgas_attachment\spms\DAR";
        // public string networkPath = @"\\192.168.2.8\pgas_photo\guest_images";
        NetworkCredential credentials = new NetworkCredential(@"pgas.ph\ranel.cator", "DomainUser1");
        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [System.Web.Services.WebMethod()]
        public DataTable get_task(int eid,string task_id,string year, string semester)
        {
            string ti = task_id.Replace("[", "");
            string tii = ti.Replace("]", "");
            string trim_task_id = tii.Replace('"',' ');
            string qry = "";
            int sem = 1;

            int currentYear = 2018;

            if (year == "" || year == null)
            {
                currentYear = DateTime.Now.Year;
            }
            else
            {
                currentYear =  Convert.ToInt32(year);
            }

            if (semester == "" || semester == null)
            {
                sem = 1;
            }
            else
            {
              sem =  Convert.ToInt32(semester);
            }
           


            if (trim_task_id == "")
            {
                qry = @"select b.id,b.Description,b.ParentID,a.TypeID,b.UserID,b.TargetID,c.semester_id,c.target_year from (select *  FROM [spms].[dbo].[spms_tblTask_Gantt] where TaskID in (select id FROM [spms].[dbo].[spms_tblTask] where UserID = '" + eid + "')) as a inner join  [spms].[dbo].[spms_tblTask] as b on a.TaskID = b.id inner join [spms].[dbo].[spms_tblIndividualTargets]  as c on b.TargetID = c.id where semester_id = '" + sem + "' and target_year = '" + currentYear + "'";
            }
            else
            {
                var ids = (@"SELECT   STUFF((SELECT ', ' +  CAST(id as nvarchar) FROM [spms].[dbo].[spms_tblTask] where  ParentID in ('" + trim_task_id + "') FOR XML PATH ('')),1,2, '') ").ScalarString();
                qry = @"select b.id,b.Description,b.ParentID,a.TypeID,b.UserID,b.TargetID,c.semester_id,c.target_year from (select *  FROM [spms].[dbo].[spms_tblTask_Gantt] where TaskID in (select id FROM [spms].[dbo].[spms_tblTask] where UserID = '" + eid + "')) as a inner join  [spms].[dbo].[spms_tblTask] as b on a.TaskID = b.id inner join [spms].[dbo].[spms_tblIndividualTargets]  as c on b.TargetID = c.id where semester_id = '" + sem + "' and target_year = '" + currentYear + "'   union select b.id,b.Description,b.ParentID,a.TypeID,b.UserID,b.TargetID,c.semester_id,c.target_year from (select *  FROM [spms].[dbo].[spms_tblTask_Gantt] where TaskID in ('"+tii+"','"+ids+"')) as a inner join  [spms].[dbo].[spms_tblTask] as b on a.TaskID = b.id inner join [spms].[dbo].[spms_tblIndividualTargets]  as c on b.TargetID = c.id where semester_id = '" + sem + "' and target_year = '" + currentYear + "'";
            }


            DataTable dt = new DataTable("task_table");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                   // SqlCommand com = new SqlCommand(@"SELECT  * from [spms].[dbo].[spms_tblTask] where UserID = '" + eid + "'", con);
                    SqlCommand com = new SqlCommand(qry, con);  
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
         
        [System.Web.Services.WebMethod()]
        public DataTable get_individualTargets(int eid,string year, string semester)
        {
            string sync_year = "";
            string sem = "";

            if (year == "" || year == null)
            {
                sync_year = DateTime.Now.Year.ToString();
            }
            else
            {
                sync_year = year;
            }
            if (semester == "" || semester == null)
            {
                sem = "1";
            }
            else
            {
                sem = semester;
            }

            DataTable dt = new DataTable("individual_target");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select a.id as individual_initiative_id, a.eid , b.id as outputid , c.id as kpm_id , Case When  c.baseline IS NULL THEN '0' ELSE c.baseline END as baseline , c.description
   ,d.description as unit, e.*
   FROM 
  [spms].[dbo].[spms_tblIndividualInitiatives] as a
  inner join [spms].[dbo].[spms_tblIndividualOutputs] as b on a.id = b.indi_initiative_id
  inner join [spms].[dbo].[spms_tblIndividualKPM] as c on b.id = c.Indivoutput_id
  inner join [spms].[dbo].[spms_tblUnitofMeasurements] as d on c.uom_id = d.id
  inner join [spms].[dbo].[spms_tblIndividualTargets] as e on c.id = e.indvkmp_id and e.target_year = " + sync_year + " and e.semester_id = " + sem + " where a.eid = '" + eid + "'", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
         
        [System.Web.Services.WebMethod()]
        
        public string SubTask_Receiver(string SQLITE_DATA, string SQLITE_PROOF, string SQLITE_MULTI_TASK, int EID)
        {
             
            string Return_msg = "";
            int si = 0;
            bool with_subtaskid = true; // yes 

            var js = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue }; 
            Subtask[] rt = js.Deserialize<Subtask[]>(SQLITE_DATA);
            Proof[] pr = js.Deserialize<Proof[]>(SQLITE_PROOF);
            Multiple_Task[] mt = js.Deserialize<Multiple_Task[]>(SQLITE_MULTI_TASK);


            DataTable dt = new DataTable();
            DataTable no_subtask_id = new DataTable();
            DataTable with_subtask_id = new DataTable();
            DataTable updated_task = new DataTable();
            DataTable proof = new DataTable();
            DataTable multi_task = new DataTable();
            try
            {

                #region datatable buildup for todos/events

                dt.Columns.Add("id");
                dt.Columns.Add("subtask_id");
                dt.Columns.Add("task_id");
                dt.Columns.Add("project_id");
                dt.Columns.Add("description");
                dt.Columns.Add("start_date");
                dt.Columns.Add("end_date");
                dt.Columns.Add("start_time");
                dt.Columns.Add("end_time");
                dt.Columns.Add("eid");
                dt.Columns.Add("is_done");
                dt.Columns.Add("is_verified");
                dt.Columns.Add("updated");
                dt.Columns.Add("actual_start_date");
                dt.Columns.Add("actual_end_date");
                dt.Columns.Add("actual_start_time");
                dt.Columns.Add("actual_end_time");
                dt.Columns.Add("start_date_time");
                dt.Columns.Add("end_date_time");
                dt.Columns.Add("target_accomplished");
                dt.Columns.Add("isalarm");
                dt.Columns.Add("alarm_option_id");
                dt.Columns.Add("output");
                dt.Columns.Add("action_code");
                dt.Columns.Add("privacy");
                dt.Columns.Add("IsTravel");
                dt.Columns.Add("ControlNoID");
                dt.Columns.Add("isppa");
                dt.Columns.Add("ppa_id");
                dt.Columns.Add("activity_id");
                dt.Columns.Add("accomplishment");
                dt.Columns.Add("date_time");
                dt.Columns.Add("is_other_funds");
                dt.Columns.Add("ppa_year");


                foreach (var s in rt.OfType<Subtask>())
                {
                    dt.Rows.Add(s.id, s.subtask_id, s.task_id, s.project_id, s.description, s.start_date, s.end_date, s.start_time, s.end_time, EID, s.is_done
                        , s.is_verified, s.updated_on, s.actual_start_date, s.actual_end_date, s.actual_start_time, s.actual_end_time, s.start_date_time, s.end_date_time, s.target_accomplished,s.isalarm,s.alarm_option_id,s.output,s.action_code,s.privacy,s.IsTravel,s.ControlNoID
                        , s.isppa, s.ppa_id, s.activity_id, s.accomplishment, s.date_time, s.is_other_funds, s.ppa_year);
                    if (s.subtask_id == 0)
                    {
                        with_subtaskid = false;
                    }
                }

                #endregion

                #region build up for proof

                proof.Columns.Add("id");
                proof.Columns.Add("picture");
                proof.Columns.Add("date_entry");
                proof.Columns.Add("event_id");
                proof.Columns.Add("subtask_id");
                proof.Columns.Add("project_id");
                proof.Columns.Add("longitude");
                proof.Columns.Add("latitude");
                proof.Columns.Add("status");
                proof.Columns.Add("mill");


                foreach (var s in pr.OfType<Proof>())
                {
                    proof.Rows.Add(s.id, s.picture, s.date_entry, s.event_id, s.subtask_id, s.project_id, s.longitude, s.latitude, s.status, s.mill);
                }

                #endregion

                #region build up for multi_task
                multi_task.Columns.Add("id");
                multi_task.Columns.Add("event_id");
                multi_task.Columns.Add("group_id");
                multi_task.Columns.Add("user_id");
              


                foreach (var s in mt.OfType<Multiple_Task>())
                {
                    multi_task.Rows.Add(s.id, s.event_id , s.group_id , EID);
                }

                #endregion

                /*
                 * First part with subtaskId
                 * Execute Stored Proc of Cator
                 * 
                 */
                #region select todos/events with subtask

                DataTable todos_with_subtask_id = dt.Clone();
                DataRow[] selected_with_subtask_id = dt.Select("NOT(subtask_id = 0)");

                foreach (DataRow row in selected_with_subtask_id)
                {
                    todos_with_subtask_id.ImportRow(row);

                    var s_id = row["subtask_id"].ToString();
                    si = Convert.ToInt32(s_id);
                    var countGroup = (@"select count(GroupID) from [spms].[dbo].[spms_tblSubTask_IsMultipleRecurring] where subtask_id = '" + s_id + "'  and UserID = '" + EID + "'").Scalar();

                    if (countGroup > 0)
                    {
                        try
                        {
                            var group_id = (@"select GroupID from [spms].[dbo].[spms_tblSubTask_IsMultipleRecurring] where subtask_id = " + s_id + "  and UserID = " + EID + "").ScalarString();


                            using (SqlConnection con = new SqlConnection(common.MyConnection()))
                            {
                                using (SqlCommand cmd = new SqlCommand(@"sp_SPMS_Utilities_ComputeProductiveHours_MultipleRecurring", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    SqlParameter param1 = new SqlParameter("@ID", SqlDbType.BigInt)
                                    {
                                        Value = Convert.ToInt64(group_id)
                                    };
                                    cmd.Parameters.Add(param1);
                                    SqlParameter param = new SqlParameter("@eid", SqlDbType.Int)
                                    {
                                        Value = EID
                                    };
                                    cmd.Parameters.Add(param);
                                    con.Open();
                                    SqlDataReader reader = cmd.ExecuteReader();
                                    con.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        using (SqlConnection con = new SqlConnection(common.MyConnection()))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"sp_SPMS_Utilities_ComputeProductiveHours", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                SqlParameter param1 = new SqlParameter("@ID", SqlDbType.Int)
                                {
                                    Value = s_id
                                };
                                cmd.Parameters.Add(param1);
                                con.Open();
                                SqlDataReader reader = cmd.ExecuteReader();
                                con.Close();
                            }
                        }
                    }
                }


                #endregion

                #region select todos/events no subtask

                DataTable todos_no_subtask_id = dt.Clone();
                DataRow[] selected_no_subtask_id = dt.Select("subtask_id = 0");

                foreach (DataRow e in selected_no_subtask_id)
                {
                    todos_no_subtask_id.ImportRow(e);
                }


                #endregion


                #region select picture with subtask_id

                DataTable picture_with_subtask_id = proof.Clone();
                DataRow[] row_picture_with_subtask_id = proof.Select("NOT(subtask_id = 0)");

                foreach (DataRow row in row_picture_with_subtask_id)
                {
                    picture_with_subtask_id.ImportRow(row);
                }

                #endregion

                #region select picture no subtask_id

                DataTable picture_no_subtask_id = proof.Clone();
                DataRow[] row_picture_no_subtask_id = proof.Select("subtask_id = 0");

                foreach (DataRow row in row_picture_no_subtask_id)
                {
                    picture_no_subtask_id.ImportRow(row);
                }

                #endregion

                /*
                 * First part without subtaskId
                 * Execute Stored Proc of Cator
                 * 
                 */

                #region Execute no subtask_id
                foreach (DataRow row in todos_no_subtask_id.Rows)
                {
                    String QRY = "";


                    var id = row["id"].ToString();
                    var subtask_id = row["subtask_id"].ToString();
                    var task_id = row["task_id"].ToString();
                    var project_id = row["project_id"].ToString();
                    var description = row["description"].ToString();
                    var start_date = row["start_date"].ToString();
                    var end_date = row["end_date"].ToString();
                    var start_time = row["start_time"].ToString();
                    var end_time = row["end_time"].ToString();
                    var eid = row["eid"].ToString();
                    var is_done = row["is_done"].ToString();
                    var is_verified = row["is_verified"].ToString();
                    var updated = row["updated"].ToString();
                    var actual_start_date = row["actual_start_date"].ToString();
                    var actual_end_date = row["actual_end_date"].ToString();
                    var actual_start_time = row["actual_start_time"].ToString();
                    var actual_end_time = row["actual_end_time"].ToString();
                    var start_date_time = row["start_date_time"].ToString();
                    var end_date_time = row["end_date_time"].ToString();
                    var target_accomplished = row["target_accomplished"].ToString();
                    var isalarm = row["isalarm"].ToString();
                    var alarm_option_id = row["alarm_option_id"].ToString();
                    var output = row["output"].ToString();
                    var action_code = row["action_code"].ToString();
                    var privacy = row["privacy"].ToString();
                    var IsTravel = row["IsTravel"].ToString();
                    var ControlNoID = row["ControlNoID"].ToString();
                    var isppa = row["isppa"].ToString();
                    var ppa_id = row["ppa_id"].ToString();
                    var activity_id = row["activity_id"].ToString();
                    var accomplishment = row["accomplishment"].ToString();
                    var date_time = row["date_time"].ToString();
                    var is_other_funds = row["is_other_funds"].ToString();
                    var ppa_year = row["ppa_year"].ToString();
                 
                    //id,task_id,project_id,subtask_description,null,null,null,null,eid,is_done,is_verified,updated_on,actual_start_date,actual_end_date,actual_start_time,actual_end_time,start_date_time,end_date_time,target_accomplished
                    var auto_approved = ("select count(*) from [spms].[dbo].[spms_tblAutoApprovedEmployees] where ActionCode = 1 and eid = '"+eid+"'").Scalar();

                    if (auto_approved > 0)
                    {
                        is_verified = "1";
                    }


                    /**
                     * Check if the data row is exist in database
                     * **/

                    var isexist = (@"select count(id) from [spms].[dbo].[spms_tblSubTask] where task_id = '" + task_id + "'  and project_id = '" + project_id + "' and subtask_description = '" + description.Replace("'", "''") + "' and start_date_time = '" + start_date_time + "' and end_date_time = '" + end_date_time + "'  and  target_accomplished = '" + target_accomplished + "' and output = '" + output + "' and eid = '" + eid + "' ").Scalar();

                    if (isexist > 0)
                    {
                        Return_msg = "succes";
                        return Return_msg;
                    }
                    else
                    { 
                    #region insert



                    if (start_date == "null" || start_date == "")
                    {
                        QRY = @"insert into [spms].[dbo].[spms_tblSubTask]  values('" + task_id + "','" + project_id + "','" + description.Replace("'", "''") + "',null,null,null,null,'" + eid + "','" + is_done + "','" + is_verified + "','" + updated + "','" + actual_start_date + "','" + actual_end_date + "','" + actual_start_time + "','" + actual_end_time + "','" + start_date_time + "','" + end_date_time + "','" + target_accomplished + "','"+isalarm+"','"+alarm_option_id+"','"+output+"','"+action_code+"','"+privacy+"','"+IsTravel+"','"+ControlNoID+"') select SCOPE_IDENTITY();";
                    }
                    else
                    {
                        QRY = @"insert into [spms].[dbo].[spms_tblSubTask]  values('" + task_id + "','" + project_id + "','" + description.Replace("'", "''") + "','" + start_date + "','" + end_date + "','" + start_time + "','" + end_time + "','" + eid + "','" + is_done + "','" + is_verified + "','" + updated + "','" + actual_start_date + "','" + actual_end_date + "','" + actual_start_time + "','" + actual_end_time + "','" + start_date_time + "','" + end_date_time + "','" + target_accomplished + "','" + isalarm + "','" + alarm_option_id + "','" + output + "','" + action_code + "','" + privacy + "','" + IsTravel + "','" + ControlNoID + "') select SCOPE_IDENTITY();";
                    }


                    //Subtask unique ID
                    int s_id = (QRY).Scalar();
                    si = s_id;

                        if (isppa == "1")
                        {
                            try
                            {
                                ("insert into [spms].[dbo].[spms_tblSubTask_PPA] values ('" + s_id + "', '" + ppa_id + "', '" + activity_id + "', '" + accomplishment + "', '" + action_code + "', '" + date_time + "', '" + eid + "', '" + is_other_funds + "', '" + ppa_year + "', null)").NonQuery();

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {

                        } 



                        foreach (DataRow r in multi_task.Rows)
                    {
                        String qry = "";
                        var m_id = r["id"].ToString();
                        var m_event_id = r["event_id"].ToString();
                        var m_group_id = r["group_id"].ToString();
                        var m_user_id = r["user_id"].ToString();

                        qry = @"insert into [spms].[dbo].[spms_tblSubTask_IsMultipleRecurring]  values ('" + s_id + "',1,'"+m_group_id+"','"+m_user_id+"')";
                        (qry).NonQuery();
                    }


                  
                    DataTable GetPicture = proof.Clone();
                    DataRow[] select_pic = proof.Select("event_id = '" + id + "'");

                    int count = 0;
                    foreach (DataRow pic_row in select_pic)
                    {
                        count++;
                        GetPicture.ImportRow(pic_row);
                    }

                    if (count > 0)
                    {
                        foreach (DataRow r in GetPicture.Rows)
                        {
                            String PIC_QRY = "";
                            var r_id = r["id"].ToString();
                            var r_picture = r["picture"].ToString();
                            var r_date_entry = r["date_entry"].ToString();
                            var r_event_id = r["event_id"].ToString();
                            var r_subtask_id = r["subtask_id"].ToString();
                            var r_longitude = r["longitude"].ToString();
                            var r_latitude = r["latitude"].ToString();
                            var r_status = r["status"].ToString();
                            var r_mill = r["mill"].ToString();

                            PIC_QRY = @"insert into [spms].[dbo].[spms_tblSubTaskProof] values('','" + r_date_entry + "','" + s_id + "','" + r_longitude + "','" + r_latitude + "','" + r_status + "','" + EID + "','png','" + r_mill + "') select SCOPE_IDENTITY();";
                                 

                            var pic_id = (PIC_QRY).Scalar();

                            (@"update [spms].[dbo].[spms_tblSubTaskProof] set attachment = '" + pic_id + "' where id = '" + pic_id + "'").NonQuery();

                            byte[] imagearr = Convert.FromBase64String(r_picture);
                            MemoryStream ms = new MemoryStream(imagearr, 0, imagearr.Length);
                            ms.Write(imagearr, 0, imagearr.Length);

                          //  var applicationPath = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority + System.Web.HttpContext.Current.Request.ApplicationPath + "/proof/" + EID;

                              
                          //  if (!Directory.Exists(Server.MapPath("~/proof/" + EID)))
                          //  {
                          //      Directory.CreateDirectory(Server.MapPath("~/proof/" + EID));
                          //  }
                             
                          //  string s = @"\";

                          //  // Convert byte[] to Image 
                          //  System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                          ////  image.Save(@"\\10.0.0.5\\proof\\" + EID +"\\"+ pic_id + ".png");
                          //  image.Save(Server.MapPath("~/proof/"+EID+"/"+pic_id+".png"));



                            //New Below

                            using (new ConnectToSharedFolder(networkPath, credentials))
                            {
                                try
                                {
                                    var newpath = networkPath + "\\" + EID;
                                    if (!Directory.Exists(newpath))
                                    {
                                        Directory.CreateDirectory(newpath);
                                    }

                                    Image img = Image.FromStream(ms , true, true);
                                    ReduceImageSizeAndSave(newpath + @"\" + pic_id + ".png", img);


                                }
                                catch (Exception ex)
                                {
                                        
                                }
                            }

                                
                                if (r_status == "end")
                            {
                                ("update [spms].[dbo].[spms_tblSubTask] set is_done = 1 where id = '" + s_id + "'").NonQuery();
                            }
                        }
                    }
                    else
                    {

                    }

                        

                        /*
                        *This part is for executing stored procedure of cator..
                        *
                        */

                        var countGroup = (@"select count(GroupID) from [spms].[dbo].[spms_tblSubTask_IsMultipleRecurring] where subtask_id = '" + s_id + "'  and UserID = '" + eid + "'").Scalar();

                    if (countGroup > 0)
                    {
                        try
                        {
                            var group_id = (@"select GroupID from [spms].[dbo].[spms_tblSubTask_IsMultipleRecurring] where subtask_id = " + s_id + "  and UserID = " + eid + "").ScalarString();


                            using (SqlConnection con = new SqlConnection(common.MyConnection()))
                            {
                                using (SqlCommand cmd = new SqlCommand(@"sp_SPMS_Utilities_ComputeProductiveHours_MultipleRecurring", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    SqlParameter param1 = new SqlParameter("@ID", SqlDbType.BigInt)
                                    {
                                        Value = Convert.ToInt64(group_id)
                                    };
                                    cmd.Parameters.Add(param1);
                                    SqlParameter param = new SqlParameter("@eid", SqlDbType.Int)
                                    {
                                        Value = EID
                                    };
                                    cmd.Parameters.Add(param);
                                    con.Open();
                                    SqlDataReader reader = cmd.ExecuteReader();
                                    con.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        using (SqlConnection con = new SqlConnection(common.MyConnection()))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"sp_SPMS_Utilities_ComputeProductiveHours", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                SqlParameter param1 = new SqlParameter("@ID", SqlDbType.Int)
                                {
                                    Value = s_id
                                };
                                cmd.Parameters.Add(param1);
                                con.Open();
                                SqlDataReader reader = cmd.ExecuteReader();
                                con.Close();
                            }
                        }
                    }


            #endregion
                    }
                }


                #endregion

                #region Execute picture with subtask_id

                DataTable GetPictureWithSubtask_id = proof.Clone();
                DataRow[] select_GetPictureWithSubtask_id = proof.Select("NOT(subtask_id = 0)");
                int count_sid = 0;
                foreach (DataRow pic_row in select_GetPictureWithSubtask_id)
                {
                    count_sid++;
                    GetPictureWithSubtask_id.ImportRow(pic_row);
                }

                if (count_sid > 0)
                {
                    var loopCounter = 0;
                    foreach (DataRow r in GetPictureWithSubtask_id.Rows)
                    {
                        String PIC_QRY = "";
                        var r_id = r["id"].ToString();
                        var r_picture = r["picture"].ToString();
                        var r_date_entry = r["date_entry"].ToString();
                        var r_event_id = r["event_id"].ToString();
                        var r_subtask_id = r["subtask_id"].ToString();
                        var r_longitude = r["longitude"].ToString();
                        var r_latitude = r["latitude"].ToString();
                        var r_status = r["status"].ToString();
                        var r_mill = r["mill"].ToString();

                        PIC_QRY = @"insert into [spms].[dbo].[spms_tblSubTaskProof] values('','" + r_date_entry + "','" + r_subtask_id + "','" + r_longitude + "','" + r_latitude + "','" + r_status + "','" + EID + "','png','" + r_mill + "') select SCOPE_IDENTITY();";

                         
                        var pic_id = (PIC_QRY).Scalar();


                        (@"update [spms].[dbo].[spms_tblSubTaskProof] set attachment = '" + pic_id + "' where id = '" + pic_id + "'").NonQuery();


                        //NEW

                        //try
                        //{
                        //    var httpRequest = HttpContext.Current.Request;

                        //    var postedFile = httpRequest.Files[loopCounter];


                        //    //var filePath = HttpContext.Current.Server.MapPath("~/proof/" + EID);
                        //    //bool exists = System.IO.Directory.Exists(filePath);

                        //    //if (!exists)
                        //    //{
                        //    //    System.IO.Directory.CreateDirectory(filePath);
                        //    //}
                        //    //postedFile.SaveAs(filePath + "/" + pic_id + ".png");


                        //    using (new ConnectToSharedFolder(networkPath, credentials))
                        //    {
                        //        try
                        //        {
                        //            var newpath = networkPath + "\\" + EID;
                        //            if (!Directory.Exists(newpath ))
                        //            {
                        //                Directory.CreateDirectory(newpath);
                        //            }

                        //            Image img = Image.FromStream(postedFile.InputStream, true, true);
                        //            ReduceImageSizeAndSave(networkPath + @"\" + pic_id + ".png", img);


                        //        }
                        //        catch (Exception ex)
                        //        {

                        //        }
                        //    }



                        //}
                        //catch (Exception ex)
                        //{
                        //    var exs =  ""+ex;
                        //}






                         
                        // //OLD
                         
                        //byte[] imagearr = System.Convert.FromBase64String(r_picture);

                        //MemoryStream ms = new MemoryStream(imagearr, 0, imagearr.Length);
                        //ms.Write(imagearr, 0, imagearr.Length);



                        //if (!Directory.Exists(Server.MapPath("~/proof/" + EID)))
                        //{
                        //    Directory.CreateDirectory(Server.MapPath("~/proof/" + EID));
                        //}

                        //string s = @"\";

                        //// Convert byte[] to Image 
                        //System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                        ////  image.Save(@"\\10.0.0.5\\proof\\" + EID +"\\"+ pic_id + ".png");
                        //image.Save(Server.MapPath("~/proof/" + EID + "/" + pic_id + ".png"));

                        //// Convert byte[] to Image 
                        ////System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                        ////image.Save(@"\\10.0.0.5\\proof\\"+EID+"\\"+ pic_id+".png");


                        byte[] imagearr = Convert.FromBase64String(r_picture);
                        MemoryStream ms = new MemoryStream(imagearr, 0, imagearr.Length);
                        ms.Write(imagearr, 0, imagearr.Length);

                        

                        using (new ConnectToSharedFolder(networkPath, credentials))
                        {
                            try
                            {
                                var newpath = networkPath + "\\" + EID;
                                if (!Directory.Exists(newpath))
                                {
                                    Directory.CreateDirectory(newpath);
                                }

                                Image img = Image.FromStream(ms, true, true);
                                ReduceImageSizeAndSave(newpath + @"\" + pic_id + ".png", img);


                            }
                            catch (Exception ex)
                            {
                                
                            }
                        } 
                         

                        if (r_status == "end")
                        {
                            ("update [spms].[dbo].[spms_tblSubTask] set is_done = 1 where id = '" + r_subtask_id + "'").NonQuery();
                        }
                    }
                }

                #endregion

                /**
                 * This part is only update if exist already in database
                 * then if its new (not inserted in database yet then it will skip this function)
                 * **/

                if(with_subtaskid == true)
                {
                #region UNION Sqlite and SqlServer With Subtask id

                DataTable UnionWithSubtaskId = dt.Clone();
                UnionWithSubtaskId.Merge(todos_with_subtask_id);

               // DataTable FromSql = get_subtask(EID);
                DataTable FromSql = get_single_subtask(EID, si);
                DataRow[] sql_row = FromSql.Select();

                foreach (DataRow sql in sql_row)
                {
                    UnionWithSubtaskId.ImportRow(sql);
                }

                DataTable Final = new DataTable();



                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    using (SqlCommand cmd = new SqlCommand(@"dodong_selectUpdatedSubtask", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter param1 = new SqlParameter("@eid", SqlDbType.Int)
                        {
                            Value = EID
                        };
                        cmd.Parameters.Add(param1);
                        SqlParameter param = new SqlParameter("@TempTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.udt_UpdatedSubTask",
                            Value = UnionWithSubtaskId
                        };
                        cmd.Parameters.Add(param);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        Final.Load(reader);
                        con.Close();
                    }
                }

                #endregion

                #region Update todos/events
                foreach (DataRow row in Final.Rows)
                {
                    string QRY_UPDATE = "";
                    //  var id = row["id"].ToString();
                    var subtask_id = row["subtask_id"].ToString();
                    var task_id = row["task_id"].ToString();
                    var project_id = row["project_id"].ToString();
                    var description = row["description"].ToString();
                    var start_date = row["start_date"].ToString();
                    var end_date = row["end_date"].ToString();
                    var start_time = row["start_time"].ToString();
                    var end_time = row["end_time"].ToString();
                    var eid = row["eid"].ToString();
                    var is_done = row["is_done"].ToString();
                    var is_verified = row["is_verified"].ToString();
                    var updated = row["updated_on"].ToString();
                    var actual_start_date = row["actual_start_date"].ToString();
                    var actual_end_date = row["actual_end_date"].ToString();
                    var actual_start_time = row["actual_start_time"].ToString();
                    var actual_end_time = row["actual_end_time"].ToString();
                    var start_date_time = row["start_date_time"].ToString();
                    var end_date_time = row["end_date_time"].ToString();
                    var target_accomplished = row["target_accomplished"].ToString();
                    var isalarm = row["isalarm"].ToString();
                    var alarm_option_id = row["alarm_option_id"].ToString();
                    var output = row["output"].ToString();
                    var action_code = row["action_code"].ToString();
                    var privacy = row["privacy"].ToString();
                    var isppa = row["isppa"].ToString();
                    var ppa_id = row["ppa_id"].ToString();
                    var activity_id = row["activity_id"].ToString();
                    var accomplishment = row["accomplishment"].ToString();
                    var date_time = row["date_time"].ToString();
                    var is_other_funds = row["is_other_funds"].ToString();
                    var ppa_year = row["ppa_year"].ToString();

                        var auto_approved = ("select count(*) from [spms].[dbo].[spms_tblAutoApprovedEmployees] where ActionCode = 1 and eid = '" + eid + "'").Scalar();

                    if (auto_approved > 0)
                    {
                        is_verified = "1";
                    }

                    if (start_date == "null" || start_date == "" )
                    {
                        QRY_UPDATE = @"update [spms].[dbo].[spms_tblSubTask] set task_id = '" + task_id + "', project_id ='" + project_id + "',  subtask_description ='" + description.Replace("'", "''") + "',start_date = null, end_date = null,start_time = null,end_time = null,eid ='" + eid + "',is_done = '" + is_done + "',is_verified ='" + is_verified + "',updated_on = '" + updated + "',actual_start_date = '" + actual_start_date + "', actual_end_date = '" + actual_end_date + "', actual_start_time = '" + actual_start_time + "', actual_end_time = '" + actual_end_time + "',start_date_time = '" + start_date_time + "',end_date_time = '" + end_date_time + "',target_accomplished = '" + target_accomplished + "' ,   isalarm = '"+isalarm+"' , alarm_option_id =  '"+alarm_option_id+"' , output = '"+output+"' , action_code = '"+action_code+"' , privacy = '"+privacy+"'  where id = '" + subtask_id + "';";
                    }
                    else
                    {
                        QRY_UPDATE = @"update [spms].[dbo].[spms_tblSubTask] set task_id = '" + task_id + "', project_id ='" + project_id + "',  subtask_description ='" + description.Replace("'", "''") + "',start_date = '" + start_date + "', end_date = '" + end_date + "',start_time = '" + start_time + "',end_time = '" + end_time + "',eid ='" + eid + "',is_done = '" + is_done + "',is_verified ='" + is_verified + "',updated_on = '" + updated + "',actual_start_date = '" + actual_start_date + "', actual_end_date = '" + actual_end_date + "', actual_start_time = '" + actual_start_time + "', actual_end_time = '" + actual_end_time + "',start_date_time = '" + start_date_time + "',end_date_time = '" + end_date_time + "',target_accomplished = '" + target_accomplished + "', isalarm = '" + isalarm + "' ,alarm_option_id = '" + alarm_option_id + "' , output = '" + output + "' , action_code = '" + action_code + "' , privacy = '" + privacy + "'  where id = '" + subtask_id + "' ;";
                    }

                        

                    (QRY_UPDATE).NonQuery();

                        if (isppa == "1")
                        {
                            try
                            {
                                ("update [spms].[dbo].[spms_tblSubTask_PPA] set ppa_id = '" + ppa_id + "' ,activity_id = '" + activity_id + "' ,accomplishment = '" + accomplishment + "' , DateTimeEntered = '" + date_time + "' , isOtherFunds = '" + is_other_funds + "', ppa_year = '" + ppa_year + "'  where subtask_id = '" + subtask_id + "' ").NonQuery();

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {

                        }  

                    }
                #endregion
                }
                Return_msg = "success";

            }
          
            catch (Exception ex)
            {
                Return_msg = "";
            }

            return Return_msg;
        }
         

        [System.Web.Services.WebMethod()]
        public DataTable CheckParameter(string username, string password)
        {
                 
            DataTable dt = new DataTable("employee_info");
            dt.Columns.Add("eid");
            dt.Columns.Add("passcode");
            dt.Columns.Add("emailaddress");
            dt.Columns.Add("fname");
            dt.Columns.Add("is_rater");
            dt.Columns.Add("officeid");
             
            try
            {
                string uname = username + "@pgas.ph";
                int eid = 0;
                string passcode = "";
                string emailaddress = "";
                string fname = "";
                int is_rater = 0;
                int officeid = 0;

                using (SqlConnection con = new SqlConnection(common.livecon()))
                {
                    SqlCommand com = new SqlCommand(@"select cast(a.eid as int) as eid,a.passcode,a.emailaddress,isnull(b.nickname,b.Firstname) as fname, b.Office from [pmis].[dbo].[vwLoginParameter] as a  
                                    inner join  [pmis].[dbo].[employee] as b on a.eid = b.eid 
                                    where a.passcode = '" + password + "' and  a.emailaddress = '" + uname + "'  ", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    { 
                        eid = (Convert.IsDBNull(reader["eid"]) ? 0 : (int)(reader["eid"]));
                        passcode = reader["passcode"] == DBNull.Value ? (string)null : (string)reader["passcode"];
                        emailaddress = reader["emailaddress"] == DBNull.Value ? (string)null : (string)reader["emailaddress"];
                        fname = reader["fname"] == DBNull.Value ? (string)null : (string)reader["fname"];
                        officeid = (Convert.IsDBNull(reader["Office"]) ? 0 : (int)(reader["Office"]));

                    } 
                    con.Close();
                }

                is_rater = isRater(eid);

                dt.Rows.Add(eid, passcode, emailaddress, fname, is_rater, officeid);


                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Services.WebMethod()]
        public DataTable get_subtask(int eid,string year, string semester)
        {
             
            int currentYear = DateTime.Now.Year;

            string add = @"<=6";
            if (semester == "2")
            {
                add = @">=7";
            }

            if(year == "" || year == null)
            {
                currentYear = DateTime.Now.Year;
            }
            else
            {
                currentYear = Convert.ToInt32(year);
            }


            DataTable dt = new DataTable("subtask");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select a.*,case when b.Remarks = '' or b.Remarks is null then 'none'  else b.Remarks end as remarks from ( select
	 id, 
     id as subtask_id,
	 task_id,
	 project_id,
	 subtask_description as description,
	 isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as start_date,
	 isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as end_date,  
	 CONVERT(VARCHAR(50),FORMAT(CAST(start_time as DateTime), 'hh:mm tt')) as start_time,
	 CONVERT(VARCHAR(50),FORMAT(CAST(end_time as DateTime), 'hh:mm tt')) as end_time, 
     eid,
	 CAST(is_done as int) as is_done  ,
	 CAST(is_verified as int ) as is_verified,
	 updated_on as updated,
	 CONVERT(VARCHAR(10), actual_start_date, 120) as actual_start_date,
	 CONVERT(VARCHAR(10), actual_end_date, 120) as actual_end_date,
	 CONVERT(VARCHAR(50),FORMAT(CAST(actual_start_time as DateTime), 'hh:mm tt')) as actual_start_time, 
	 CONVERT(VARCHAR(50),FORMAT(CAST(actual_end_time as DateTime), 'hh:mm tt')) as actual_end_time,  
	 CONVERT(varchar(20),start_date_time , 120 ) as start_date_time,
	 CONVERT(varchar(20),end_date_time , 120 ) as end_date_time,
	 target_accomplished,
     isalarm,
     alarm_option_id,
     output,      
     action_code, 
     privacy, 
     IsTravel,
     Case when ControlNoID = '0' or ControlNoID = '' then '0' else ControlNoID  END as ControlNoID
     from [spms].[dbo].[spms_tblSubTask] where eid = '" + eid + "' and action_code = 1 and subtask_description != '' and MONTH(actual_end_date) " + add.Replace("'", "") + " and YEAR(actual_start_date) = '" + currentYear + "' ) as a left join ( select * from ( select distinct SubtaskID,Remarks,ROW_NUMBER() Over (Partition By SubtaskID Order By id Desc) As Rn  from [spms].[dbo].[spms_tblAccomplishmentStatus_Logs]) as a where Rn = 1) as b on a.id = b.SubtaskID  order by actual_start_date  ", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
         
        [System.Web.Services.WebMethod()]
        public DataTable get_Proof_Ref(int eid)
        {
            DataTable dt = new DataTable("proof_ref");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select subtask_id,status FROM [spms].[dbo].[spms_tblSubTaskProof] where eid = '" + eid + "' ", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Services.WebMethod()]
        public DataTable get_alarmOption()
        {
            DataTable dt = new DataTable("alarmOption");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select * from [spms].[dbo].[spms_tblSubTaskAlarmOption]", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Services.WebMethod()]
        public DataTable NewUpdate()
        {
            DataTable dt = new DataTable("availableUpdate");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select 12 as update_available,'We recommend to sync your data first before updating.    <br>Need assistance ? Dont hesitate to call us..<br> <br>Just dial 310 on your ip phone for technical assistance. Thank you' as text", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class Subtask
        {
            public Int64 id { get; set; }
            public int subtask_id { get; set; }
            public int task_id { get; set; }
            public int project_id { get; set; }
            public string description { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
            public string start_time { get; set; }
            public string end_time { get; set; }
            public int eid { get; set; }
            public int is_done { get; set; }
            public int is_verified { get; set; }
            public string updated_on { get; set; }
            public string actual_start_date { get; set; }
            public string actual_end_date { get; set; }
            public string actual_start_time { get; set; }
            public string actual_end_time { get; set; }
            public string start_date_time { get; set; }
            public string end_date_time { get; set; }
            public double target_accomplished { get; set; }
            public int isalarm { get; set; }
            public int alarm_option_id { get; set; }
            public string output { get; set; } 
            public int action_code { get; set; } 
            public int privacy { get; set; }
            public int IsTravel { get; set; }
            public string ControlNoID { get; set; }
            public string remarks { get; set; }
            public int isppa { get; set; }
            public int ppa_id { get; set; }
            public int activity_id { get; set; }
            public int accomplishment { get; set; }
            public string date_time { get; set; }
            public int is_other_funds { get; set; }
            public int ppa_year { get; set; }
        }
        public class Proof
        {
            public int id { get; set; }
            public string picture { get; set; }
            public string date_entry { get; set; }
            public int event_id { get; set; }
            public int subtask_id { get; set; }
            public int project_id { get; set; }
            public double longitude { get; set; }
            public double latitude { get; set; } 
            public string status { get; set; }
            public Int64 mill { get; set; }

        }

        public class Multiple_Task
        {
            public int id { get; set; }
            public string event_id { get; set; }
            public string group_id { get; set; }
            public int user_id { get; set; }

        }

        public class infra
        {
            public int project_id { get; set; }
            public string title { get; set; }
            public string location { get; set; } 
            public string date_started { get; set; }
            public string date_ended { get; set; }
            public string time_started { get; set; }
            public string time_ended { get; set; }
            public string am_1 { get; set; }
            public string am_2 { get; set; }
            public string am_3 { get; set; }
            public string am_4 { get; set; }
            public string am_5 { get; set; }
            public string am_6 { get; set; }
            public string am_7 { get; set; }
            public string am_8 { get; set; }
            public string am_9 { get; set; }
            public string am_10 { get; set; }
            public string am_11 { get; set; }
            public string am_12 { get; set; }
            public string pm_1 { get; set; }
            public string pm_2 { get; set; }
            public string pm_3 { get; set; }
            public string pm_4 { get; set; }
            public string pm_5 { get; set; }
            public string pm_6 { get; set; }
            public string pm_7 { get; set; }
            public string pm_8 { get; set; }
            public string pm_9 { get; set; }
            public string pm_10 { get; set; }
            public string pm_11 { get; set; }
            public string pm_12 { get; set; }
            public string project_manager { get; set; }
            public string project_engineer { get; set; }
            public string materials_engineer { get; set; }
            public string safety_engineer { get; set; }
            public string survey_engineer { get; set; }
            public string office_engineer { get; set; }
            public string construction_foreman { get; set; }
            public string he_operator { get; set; }
            public string drivers { get; set; }
            public string laborers { get; set; }
            public string mason { get; set; }
            public string carpenter { get; set; }
            public string material_remarks { get; set; }
            public string work_progress { get; set; }
            public string start_date_time { get; set; }
            public string end_date_time { get; set; }
            public string updated_on { get; set; }
             
        }

        public class infra_equipment
        {
            public string ppaid { get; set; }
            public string equipment { get; set; }
            public string qty { get; set; }
            public string operating { get; set; }
            public string standby { get; set; }
            public string breakdown { get; set; } 

        }

        public class infra_materials
        {
            public string ppaid { get; set; }
            public string location { get; set; }
            public string description { get; set; }
            public string qty { get; set; }
            public string unit { get; set; }
            public string accepted { get; set; }
            public string rejected { get; set; }

        }

        public class infra_attachment
        {
            public string id { get; set; }
            public string picture { get; set; }
            public string date_entry { get; set; }
            public string ppaid { get; set; }
            public string position_name { get; set; } 
            public double longitude { get; set; }
            public double latitude { get; set; } 
            public string eid { get; set; } 
            public Int64 mill { get; set; }

        }

        public class infra_findings
        {
            public string ppaid { get; set; }
            public string findings { get; set; }
            public string recommendations { get; set; } 

        }

        [System.Web.Services.WebMethod()]
        public DataTable jsonfeed(int stat, int eid)
        {
            //SQlite
            // 0 is public
            // 1 is office only
           

            //SqlServer
            //1 is public
            //2 is office only



            string qry = "";

         //   if (stat == 1)
            //{
            var officeid = (@"select Department FROM [pmis].[dbo].[m_vwGetAllEmployee_Minified] where eid = '" + eid + "'").Scalar();
            qry = @"select tbl_1.*,tbl_2.*,Case when tbl_3.likes is not null then tbl_3.likes else '0' end as likes, Case when tbl_4.subtask_id is not null then '1' else '0' end as isLiked,tbl_5.EmpFullName as liker,tbl_5.liker_eid from ( SELECT top 50 subtask_id, STUFF((SELECT ',' + CONCAT(' 10.0.0.5/proof/',eid,'/',attachment,'.',attachment_extension) FROM spms_tblSubTaskProof as t2 WHERE t1.subtask_id = t2.subtask_id FOR XML PATH ('')),1,2, '') as image FROM spms_tblSubTaskProof as t1 LEFT JOIN spms_tblSubtask as a ON t1.subtask_id = a.id  where a.Privacy = 2 GROUP BY t1.subtask_id order by t1.subtask_id desc) as tbl_1 inner join   ( select  a1.updated_on as timeStamp,a1.id,a1.subtask_description as status,'http://192.168.2.104/hris/Content/images/photos/'+cast(a1.eid as nvarchar(20) ) +'.png' as profilePic,b1.EmpFullName as name,b1.Department from [spms].[dbo].[spms_tblSubTask]  as a1  inner join  [pmis].[dbo].[m_vwGetAllEmployee_Minified] as b1 on a1.eid = b1.eid  where a1.action_code = 1 ) as tbl_2 on tbl_1.subtask_id = tbl_2.id left join ( select count(subtask_id) as likes , subtask_id FROM [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes]group by subtask_id ) as tbl_3  on tbl_1.subtask_id = tbl_3.subtask_id left join ( select distinct(subtask_id) from [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes]  where eid = " + eid + " ) as tbl_4 on tbl_1.subtask_id = tbl_4.subtask_id left join ( select a.*,b.EmpFullName,b.eid as liker_eid from (select ROW_NUMBER() Over ( partition by subtask_id order by id desc ) rn ,subtask_id,eid from  [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes]   ) as a inner join  [pmis].[dbo].[m_vwGetAllEmployee_Minified]  as b on a.eid = b.eid where a.rn = 1)  as tbl_5 on tbl_1.subtask_id = tbl_5.subtask_id   where Department = " + officeid + " order by timeStamp desc";
          //  }
           // else
           // {
            //    qry = @"select tbl_1.*,tbl_2.*,Case when tbl_3.likes is not null then tbl_3.likes else '0' end as likes, Case when tbl_4.subtask_id is not null then '1' else '0' end as isLiked,tbl_5.EmpFullName as liker,tbl_5.liker_eid from ( SELECT top 100 subtask_id, STUFF((SELECT ',' + CONCAT(' 10.0.0.5/proof/',eid,'/',attachment,'.',attachment_extension) FROM spms_tblSubTaskProof as t2 WHERE t1.subtask_id = t2.subtask_id FOR XML PATH ('')),1,2, '') as image FROM spms_tblSubTaskProof as t1 LEFT JOIN spms_tblSubtask as a ON t1.subtask_id = a.id  where a.Privacy = 1 GROUP BY t1.subtask_id order by t1.subtask_id desc) as tbl_1 inner join   ( select  a1.updated_on as timeStamp,a1.id,a1.subtask_description as status,'http://192.168.2.104/hris/Content/images/photos/'+cast(a1.eid as nvarchar(20) ) +'.png' as profilePic,b1.EmpFullName as name,b1.Department from [spms].[dbo].[spms_tblSubTask]  as a1  inner join  [pmis].[dbo].[m_vwGetAllEmployee] as b1 on a1.eid = b1.eid  where a1.action_code = 1 ) as tbl_2 on tbl_1.subtask_id = tbl_2.id left join ( select count(subtask_id) as likes , subtask_id FROM [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes]group by subtask_id ) as tbl_3  on tbl_1.subtask_id = tbl_3.subtask_id left join ( select distinct(subtask_id) from [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes]  where eid = " + eid + " ) as tbl_4 on tbl_1.subtask_id = tbl_4.subtask_id left join ( select a.*,b.EmpFullName,b.eid as liker_eid from (select ROW_NUMBER() Over ( partition by subtask_id order by id desc ) rn ,subtask_id,eid from  [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes]   ) as a inner join  [pmis].[dbo].[m_vwGetAllEmployee]  as b on a.eid = b.eid where a.rn = 1)  as tbl_5 on tbl_1.subtask_id = tbl_5.subtask_id order by timeStamp desc";
            //}

            DataTable dt = new DataTable("feed");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(qry, con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Services.WebMethod()]
        public DataTable GetTravel(int eid,string year, string semester)
        {
         // string qry = @"select a.trid_emp,'['+b.ControlNo+'] '+b.Purpose+ ' @ ' +b.Destination  as title,a.eid ,CONVERT(VARCHAR(10), MIN(b.InclusiveDate), 120) as min_date, CONVERT(VARCHAR(10), MAX(b.InclusiveDate), 120) as max_date  from [pmis].[dbo].[m_tblTravelOrder_Employees] as a left join  [pmis].[dbo].[m_tblTravelOrder_Details] as b on a.ControlNo = b.ControlNo left join [pmis].[dbo].[m_tblTravelOrder] as c on a.ControlNo = c.ControlNo where c.IsApproved = 1 and a.eid = '"+eid+"'  group by a.trid_emp,b.ControlNo,a.eid ,c.IsApproved,b.Purpose,b.Destination"; 

            int currentYear = 2018;

            if (year == "" || year == null)
            {
                currentYear = DateTime.Now.Year;
            }
            else
            {
                currentYear =  Convert.ToInt32(year);
            }


            DataTable final = new DataTable("TO");
            final.Columns.Add("trid_emp", typeof(string));
            final.Columns.Add("title",typeof(string));
            final.Columns.Add("eid", typeof(int));
            final.Columns.Add("min_date", typeof(string));
            final.Columns.Add("max_date", typeof(string));


            DataTable dt1 = (@"select b.ControlNo as trid_emp,'['+b.ControlNo+'] '+b.Purpose+ ' @ ' +b.Destination  as title,CAST(a.eid as int) as eid , CONVERT(VARCHAR(10), MIN(b.InclusiveDate), 120) as min_date, CONVERT(VARCHAR(10), MAX(b.InclusiveDate), 120) as max_date  from [pmis].[dbo].[m_tblTravelOrder_Employees] as a left join  [pmis].[dbo].[m_tblTravelOrder_Details] as b on a.ControlNo = b.ControlNo left join [pmis].[dbo].[m_tblTravelOrder] as c on a.ControlNo = c.ControlNo where c.IsApproved = 1 and a.eid = '" + eid + "' and  YEAR(b.InclusiveDate) = '" + currentYear + "'   group by a.trid_emp,b.ControlNo,a.eid ,c.IsApproved,b.Purpose,b.Destination ").DataSet();

            DataTable dt2 = (@"select a.ControlNo as trid_emp,'['+a.ControlNo+'] '+b.Purpose+ ' @ ' +b.Destination as title,CAST(a.RequestingOfficial as int) as eid,CONVERT(VARCHAR(10), MIN(b.InclusiveDate), 120) as min_date , CONVERT(VARCHAR(10), MAX(b.InclusiveDate), 120) as max_date FROM pmis.dbo.m_tblPermitToLeaveStation as a left join pmis.dbo.m_tblPermitToLeaveStation_InclusiveDates as b on a.ControlNo = b.ControlNo where a.IsApproved = 1 and a.RequestingOfficial = '" + eid + "' and YEAR(b.InclusiveDate) = '" + currentYear + "'   group by a.PermitId,a.ControlNo,a.RequestingOfficial,a.IsApproved,b.Purpose,b.Destination").DataSet();

            DataTable dt3 = (@"select a.ControlNo as trid_emp ,'['+a.ControlNo+'] '+a.Purpose+ ' @ ' +a.DestPlace as title,CAST(a.DriverEid as int) as eid,CONVERT(VARCHAR(10), MIN(b.InclusiveDate), 120) as min_date , CONVERT(VARCHAR(10), MAX(b.InclusiveDate), 120) as max_date from  pmis.dbo.m_tblTripTicket as a left join pmis.dbo.m_tblTripTicket_Inclusive as b on a.ControlNo = b.ControlNo where isApproved = 1 and DriverEid = '" + eid + "' and YEAR(b.InclusiveDate) = '" + currentYear + "' group by a.TrId,a.ControlNo,a.DriverEid,a.isApproved,a.Purpose,a.DestPlace").DataSet();

            final.Merge(dt1);
            final.Merge(dt2);
            final.Merge(dt3);

            /*
            DataTable dt = new DataTable("TO");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(qry, con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            */
            return final;
        }

        [System.Web.Services.WebMethod()]
        public DataTable get_names(string name)
        {
            string qry = @"select EmpName,eid from [pmis].[dbo].[vwMergeAllEmployee] where EmpName like '%"+name+"%'";
            DataTable dt = new DataTable("names");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(qry, con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [System.Web.Services.WebMethod()]
        public DataTable get_allnames()
        {
            string qry = @"select EmpName,eid from [pmis].[dbo].[vwMergeAllEmployee] ";
            DataTable dt = new DataTable("names");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(qry, con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Services.WebMethod()]
        public DataTable get_other_task(int eid)
        {
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;

            int sem = 1;
            if (currentMonth > 6)
            {
                sem = 2;
            }

            DataTable dt = new DataTable("task_table");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    // SqlCommand com = new SqlCommand(@"SELECT  * from [spms].[dbo].[spms_tblTask] where UserID = '" + eid + "'", con);
                    SqlCommand com = new SqlCommand(@"select b.id,b.Description,b.ParentID,a.TypeID,b.UserID,b.TargetID,c.semester_id,c.target_year from (select *  FROM [spms].[dbo].[spms_tblTask_Gantt] where TaskID in (select id FROM [spms].[dbo].[spms_tblTask] where UserID = '" + eid + "')) as a inner join  [spms].[dbo].[spms_tblTask] as b on a.TaskID = b.id inner join [spms].[dbo].[spms_tblIndividualTargets]  as c on b.TargetID = c.id where semester_id = '" + sem + "' and target_year = '" + currentYear + "'", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Services.WebMethod()]
        public DataTable getForApproval(int eid )
        {  
            DataTable todos = new DataTable("todos");
            String ss = "";
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                var query = @"dbo.sp_SPMS_ApprovalContent_v2 " + eid + ",0,'" + ss + "'";

                SqlCommand comm = new SqlCommand(query, con); 
                con.Open();
               
                    SqlDataReader reader = comm.ExecuteReader();
                    todos.Load(reader); 
            }
            return todos;
        }

        [System.Web.Services.WebMethod()]
        public int isRater(int eid)
        { 
            string qry = @"Declare @officeID int = this_officeid; 

with x as (select isnull((select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.raterID and
OfficeID = a.OfficeID and a.eid != a.raterID),
isnull((select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.RaterID and OfficeID = @OfficeID),
(select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.RaterID ))) as ParentSeriesID
from spms_tblOrganizationalChart as a
left join pmis.dbo.Employee as b on b.eid = a.eid
left join pmis.dbo.refsPositions as c on c.PositionCode = a.PositionID
left join pmis.dbo.OfficeDescription as d on d.OfficeID = a.OfficeID
left join spms_tblOffice as e on e.OfficeID = a.OfficeID
left join pmis.dbo.EDGE_tblPlantillaDivision as f on f.DivID = a.DivisionID
left join pmis.dbo.m_vwGetAllEmployee as g on g.eid = a.eid and g.isactive = 1
where a.OfficeID in(@officeID) 
)
select a.SeriesID,isnull((select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.raterID and
OfficeID = a.OfficeID and a.eid != a.raterID),
isnull((select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.RaterID and OfficeID = @OfficeID),
(select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.RaterID ))) as ParentSeriesID,
b.Firstname + ' ' + left(mi,1) + '. ' + Lastname as EmployeeName, isnull(g.Position,c.Pos_name) as Position,
a.eid,d.OfficeAbbr,e.OfficeColor,f.DivName,a.RaterID,
a.PositionID,a.DivisionID,a.OfficeID,
case when a.eid in (select RaterID from spms_tblOrganizationalChart) then 1 else 0 end as isRater,
case when a.eid in (select RaterID from spms_tblOrganizationalChart) and a.OfficeID is not null 
then 'No. of Subordinates : ' + cast((select count(RaterID) from spms_tblOrganizationalChart where raterID = a.eid)
as varchar(10)) + '/' + cast((select count(RaterID) from spms_tblOrganizationalChart 
where OfficeID = a.OfficeID) as varchar(10)) else '' end as Subordinates
from spms_tblOrganizationalChart as a
left join pmis.dbo.Employee as b on b.eid = a.eid
left join pmis.dbo.refsPositions as c on c.PositionCode = a.PositionID
left join pmis.dbo.OfficeDescription as d on d.OfficeID = a.OfficeID
left join spms_tblOffice as e on e.OfficeID = a.OfficeID
left join pmis.dbo.EDGE_tblPlantillaDivision as f on f.DivID = a.DivisionID
left join pmis.dbo.m_vwGetAllEmployee as g on g.eid = a.eid and g.isactive = 1
where a.OfficeID in(@officeID) or a.SeriesID in(select x.ParentSeriesID from x) Order by a.DivisionID";



            int officeid = (@"select OfficeID from spms_tblOrganizationalChart where eid = " + eid).Scalar();

            DataTable Employees = (qry.Replace("this_officeid", officeid.ToString())).DataSet();

            DataRow[] select_row_data_rater = Employees.Select("eid = " + eid + "");

            int stat = 0;

            foreach (DataRow row in select_row_data_rater)
            {

                int israter = Convert.ToInt32(row["isRater"].ToString());

                if (israter == 1)
                {
                    stat = 1;
                }
            }
            return stat;
        }
        

        [System.Web.Services.WebMethod()]
        public DataTable GetImageIds(string subtask_ids)
        {

            char[] MyChar = { '[', ']'};
            string NewString = subtask_ids.Trim(MyChar);

            DataTable ids = new DataTable("imageIds");

            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"SELECT subtask_id, STUFF((SELECT ',' + CONCAT(' 10.0.0.5/proof/',eid,'/',attachment,'.',attachment_extension)
FROM spms_tblSubTaskProof as t2
WHERE t1.subtask_id = t2.subtask_id
FOR XML PATH ('')),1,2, '') as ImageID
FROM spms_tblSubTaskProof as t1
LEFT JOIN spms_tblSubtask as a ON t1.subtask_id = a.id   
where t1.subtask_id in (" + NewString + ") GROUP BY t1.subtask_id", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    ids.Load(reader);
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            } 
            return ids;
        }

        [System.Web.Services.WebMethod()]
        public string SubmitApproved(string APPROVED,int eid)
        {
            string Return_msg = "";

            var js = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
            Subtask[] rt = js.Deserialize<Subtask[]>(APPROVED);
           
            DataTable dt = new DataTable();
            dt.Columns.Add("subtask_id", typeof(int));
            dt.Columns.Add("is_done", typeof(int));
            dt.Columns.Add("is_verified", typeof(int));
            dt.Columns.Add("updated", typeof(Int64));
            dt.Columns.Add("remarks", typeof(string));
            try
            {
                int count = 0;

                //count how may ids
                foreach (var s in rt.OfType<Subtask>())
                {
                     count++; 
                }

                String[] ids = new String[count];

                int loop_id = 0;
                foreach (var s in rt.OfType<Subtask>())
                {
                    ids[loop_id] = s.id.ToString();
                    dt.Rows.Add(s.id,s.is_done,s.is_verified,s.updated_on,s.remarks);
                    loop_id++;
                }


                DataTable dt1 = (@"select CAST(id as int) as subtask_id ,CAST(is_done as int) as is_done  ,
	 CAST(is_verified as int ) as is_verified,
	 updated_on as updated ,'' as remarks from  [spms].[dbo].[spms_tblSubTask] where id in ("+String.Join(",", ids)+")").DataSet();
                dt.Merge(dt1); 

               DataTable Final = new DataTable();
             

                 
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    using (SqlCommand cmd = new SqlCommand(@"sp_SPMS_SynSelectUpdated", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter param = new SqlParameter("@TempTable", SqlDbType.Structured)
                        {
                            TypeName = "[dbo].[udt_ApproveAccomplishment]",
                            Value = dt
                        };
                        cmd.Parameters.Add(param);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        Final.Load(reader);
                        con.Close();
                    }
                }

                foreach (DataRow row in Final.Rows)
                {
                    string QRY_UPDATE = ""; 
                    var subtask_id = row["subtask_id"].ToString(); 
                    var is_done = row["is_done"].ToString();
                    var is_verified = row["is_verified"].ToString();
                    var updated = row["updated_on"].ToString();
                    var remarks = row["remarks"].ToString();

                    if (remarks.Length > 0)
                    {

                        int count1 = (@"select count(id) from [spms].[dbo].[spms_tblSubtask_Remarks] where subtask_id = " + subtask_id + "").Scalar();

                        if (count1 > 0)
                        {
                            (@"update [spms].[dbo].[spms_tblSubtask_Remarks] set remarks = '"+remarks+"' where subtask_id = "+subtask_id+"").NonQuery();
                        }
                        else
                        {
                            (@"insert into [spms].[dbo].[spms_tblSubtask_Remarks]  values ("+subtask_id+",'"+remarks+"')").NonQuery();
                        }
                        var qry = "insert into [spms].[dbo].[spms_tblAccomplishmentStatus_Logs] values ('" + subtask_id + "','" + eid + "','" + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "',1,1,'" + remarks + "')";
                       (qry).NonQuery();
                    }
                   
                   QRY_UPDATE = @"update [spms].[dbo].[spms_tblSubTask] set is_done = '" + is_done + "' , is_verified = '" + is_verified +"',updated_on = '" + updated + "'   where id = '" + subtask_id + "';";
                    
                   (QRY_UPDATE).NonQuery(); 
                }

                Return_msg = "success";

            }
            catch (Exception ex)
            {
                Return_msg = ""+ex;
            }

            return Return_msg;
        }

        public DataTable get_single_subtask(int eid,int subtask_id)
        {
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;

            string add = @"<=6";
            if (currentMonth > 6)
            {
                add = @">=7";
            }

            DataTable dt = new DataTable("subtask");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select a.*,case when b.Remarks = '' or b.Remarks is null then 'none'  else b.Remarks end as remarks from (select id,id as subtask_id,task_id,project_id,subtask_description as description,isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as start_date,isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as end_date,  CONVERT(VARCHAR(50),FORMAT(CAST(start_time as DateTime), 'hh:mm tt')) as start_time,CONVERT(VARCHAR(50),FORMAT(CAST(end_time as DateTime), 'hh:mm tt')) as end_time, eid,CAST(is_done as int) as is_done  ,CAST(is_verified as int ) as is_verified,updated_on as updated,CONVERT(VARCHAR(10), actual_start_date, 120) as actual_start_date,CONVERT(VARCHAR(10), actual_end_date, 120) as actual_end_date,CONVERT(VARCHAR(50),FORMAT(CAST(actual_start_time as DateTime), 'hh:mm tt')) as actual_start_time, CONVERT(VARCHAR(50),FORMAT(CAST(actual_end_time as DateTime), 'hh:mm tt')) as actual_end_time,  CONVERT(varchar(20),start_date_time , 120 ) as start_date_time,CONVERT(varchar(20),end_date_time , 120 ) as end_date_time,target_accomplished,isalarm,alarm_option_id,output , action_code, privacy, IsTravel,Case when ControlNoID = '0' or ControlNoID = '' then '0' else ControlNoID  END as ControlNoID from [spms].[dbo].[spms_tblSubTask] where eid = '" + eid + "'and id = '"+subtask_id+"' and action_code = 1 and subtask_description != '' ) as a left join ( select * from ( select distinct SubtaskID,Remarks,ROW_NUMBER() Over (Partition By SubtaskID Order By id Desc) As Rn  from [spms].[dbo].[spms_tblAccomplishmentStatus_Logs]) as a where Rn = 1) as b on a.id = b.SubtaskID  order by actual_start_date  ", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                //and MONTH(actual_end_date) " + add.Replace("'", "") + " and YEAR(actual_start_date) = '" + currentYear + "' )
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Services.WebMethod()]
        public string sendLike(int subtask_id,int eid)
        {
            try
            {
                var count = ("select count(*) from [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes] where subtask_id = " + subtask_id + " and eid = " + eid + "").Scalar();
                if (count > 0)
                {
                    ("delete [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes] where subtask_id = " + subtask_id + " and eid = " + eid + "").NonQuery();
                }
                else
                {
                    ("insert into [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes] (subtask_id,eid) values(" + subtask_id + "," + eid + ")").NonQuery();
                }  
                return "1";
            }
            catch (Exception ex)
            {
                return "0";
            } 
        }
         [System.Web.Services.WebMethod()]
        public DataTable get_likers(int subtask_id)
        { 
            DataTable dt = new DataTable("likers");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select a.*,b.EmpFullName from [spms].[dbo].[spms_tblNewsfeed_ItemUpvotes] as a  inner join [pmis].[dbo].[m_vwGetAllEmployee]  as b on a.eid = b.eid  where subtask_id = " + subtask_id + "", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region For Meeting

        [System.Web.Services.WebMethod()]
        public DataTable getMeeting(int eid)
        { 
            DataTable dt = new DataTable("meeting");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select id,description,isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as start_date,isnull(CONVERT(VARCHAR(10), end_date, 120),NULL) as end_date,CONVERT(VARCHAR(50),FORMAT(CAST(start_time as DateTime), 'hh:mm tt')) as start_time,CONVERT(VARCHAR(50),FORMAT(CAST(end_time as DateTime), 'hh:mm tt')) as end_time from  [spms].[dbo].[spms_tblCalendarOfActivities] where id in (select a.id  from [spms].[dbo].[spms_tblCalendarOfActivities] as a inner join [spms].[dbo].[spms_tblMeetingParticipants] as b on a.id = b.Meeting_id where b.requested_by = " + eid + " group by a.id,b.requested_by) order by start_date desc", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                } 
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [WebMethod]
        public string uploadInfraMonitoring(string json, string jsonequipment, string jsonmaterials, string jsonfindings, string attachment, string EID)
        {
            string ret = "";
            String QRY = "";
            int s_id = 0;

            var js = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
            infra[] rd = js.Deserialize<infra[]>(json);
            infra_equipment[] ie = js.Deserialize<infra_equipment[]>(jsonequipment);
            infra_materials[] im = js.Deserialize<infra_materials[]>(jsonmaterials);
            infra_findings[] findings = js.Deserialize<infra_findings[]>(jsonfindings);
            infra_attachment[] attach = js.Deserialize<infra_attachment[]>(attachment);
          

            DataTable dt = new DataTable();
            DataTable dtp = new DataTable();
            DataTable dtm = new DataTable();
            DataTable dtf = new DataTable();
            DataTable dtattach = new DataTable();

            try
            {
                dt.Columns.Add("project_id");
                dt.Columns.Add("title");
                dt.Columns.Add("location");
                dt.Columns.Add("date_started");
                dt.Columns.Add("date_ended");
                dt.Columns.Add("time_started");
                dt.Columns.Add("time_ended");
                dt.Columns.Add("am_1");
                dt.Columns.Add("am_2");
                dt.Columns.Add("am_3");
                dt.Columns.Add("am_4");
                dt.Columns.Add("am_5");
                dt.Columns.Add("am_6");
                dt.Columns.Add("am_7");
                dt.Columns.Add("am_8");
                dt.Columns.Add("am_9");
                dt.Columns.Add("am_10");
                dt.Columns.Add("am_11");
                dt.Columns.Add("am_12");
                dt.Columns.Add("pm_1");
                dt.Columns.Add("pm_2");
                dt.Columns.Add("pm_3");
                dt.Columns.Add("pm_4");
                dt.Columns.Add("pm_5");
                dt.Columns.Add("pm_6");
                dt.Columns.Add("pm_7");
                dt.Columns.Add("pm_8");
                dt.Columns.Add("pm_9");
                dt.Columns.Add("pm_10");
                dt.Columns.Add("pm_11");
                dt.Columns.Add("pm_12");
                dt.Columns.Add("project_manager");
                dt.Columns.Add("project_engineer");
                dt.Columns.Add("materials_engineer");
                dt.Columns.Add("safety_engineer");
                dt.Columns.Add("survey_engineer");
                dt.Columns.Add("office_engineer");
                dt.Columns.Add("construction_foreman");
                dt.Columns.Add("he_operator");
                dt.Columns.Add("drivers");
                dt.Columns.Add("laborers");
                dt.Columns.Add("mason");
                dt.Columns.Add("carpenter");
                dt.Columns.Add("material_remarks");
                dt.Columns.Add("work_progress");
                dt.Columns.Add("start_date_time");
                dt.Columns.Add("end_date_time");
                dt.Columns.Add("updated_on");


                foreach (var s in rd.OfType<infra>())
                {
                    dt.Rows.Add(s.project_id, s.title, s.location, s.date_started, s.date_ended, s.time_started, s.time_ended, s.am_1, s.am_2, s.am_3, s.am_4, s.am_5, s.am_6, s.am_7,
                        s.am_8, s.am_9, s.am_10, s.am_11, s.am_12, s.pm_1, s.pm_2, s.pm_3, s.pm_4, s.pm_5, s.pm_6, s.pm_7, s.pm_8, s.pm_9, s.pm_10, s.pm_11, s.pm_12, s.project_manager,
                        s.project_engineer, s.materials_engineer, s.safety_engineer, s.survey_engineer, s.office_engineer, s.construction_foreman, s.he_operator, s.drivers,
                        s.laborers, s.mason, s.carpenter, s.material_remarks, s.work_progress, s.start_date_time, s.end_date_time, s.updated_on);

                }

                

                dtattach.Columns.Add("id");
                dtattach.Columns.Add("picture");
                dtattach.Columns.Add("date_entry");
                dtattach.Columns.Add("ppaid");
                dtattach.Columns.Add("position_name");
                dtattach.Columns.Add("longitude");
                dtattach.Columns.Add("latitude");
                //dtattach.Columns.Add("eid");
                dtattach.Columns.Add("mill");

                foreach (var s in attach.OfType<infra_attachment>())
                {
                    dtattach.Rows.Add(s.id, s.picture, s.date_entry, s.ppaid, s.position_name, s.longitude, s.latitude, s.mill);
                }


                DataTable picture_with_subtask_id = dtattach.Clone();
                DataRow[] row_picture_with_subtask_id = dtattach.Select("NOT(ppaid = 0)");

                foreach (DataRow row in row_picture_with_subtask_id)
                {
                    picture_with_subtask_id.ImportRow(row);
                }

                foreach (DataRow row in dt.Rows)
                {
                    
                    var project_id = row["project_id"].ToString();
                    var title = row["title"].ToString();
                    var location = row["location"].ToString();
                    var date_started = row["date_started"].ToString();
                    var date_ended = row["date_ended"].ToString();
                    var time_started = row["time_started"].ToString();
                    var time_ended = row["time_ended"].ToString();
                    var am_1 = row["am_1"].ToString(); 
                    var am_2 = row["am_2"].ToString(); 
                    var am_3 = row["am_3"].ToString(); 
                    var am_4 = row["am_4"].ToString(); 
                    var am_5 = row["am_5"].ToString(); 
                    var am_6 = row["am_6"].ToString(); 
                    var am_7 = row["am_7"].ToString(); 
                    var am_8 = row["am_8"].ToString(); 
                    var am_9 = row["am_9"].ToString(); 
                    var am_10 = row["am_10"].ToString(); 
                    var am_11 = row["am_11"].ToString(); 
                    var am_12 = row["am_12"].ToString();
                    var pm_1 = row["pm_1"].ToString();
                    var pm_2 = row["pm_2"].ToString();
                    var pm_3 = row["pm_3"].ToString();
                    var pm_4 = row["pm_4"].ToString();
                    var pm_5 = row["pm_5"].ToString();
                    var pm_6 = row["pm_6"].ToString();
                    var pm_7 = row["pm_7"].ToString();
                    var pm_8 = row["pm_8"].ToString();
                    var pm_9 = row["pm_9"].ToString();
                    var pm_10 = row["pm_10"].ToString();
                    var pm_11 = row["pm_11"].ToString();
                    var pm_12 = row["pm_12"].ToString();
                    var project_manager = row["project_manager"].ToString();
                    var project_engineer = row["project_engineer"].ToString();
                    var materials_engineer = row["materials_engineer"].ToString();
                    var safety_engineer = row["safety_engineer"].ToString();
                    var survey_engineer = row["survey_engineer"].ToString();
                    var office_engineer = row["office_engineer"].ToString();
                    var construction_foreman = row["construction_foreman"].ToString();
                    var he_operator = row["he_operator"].ToString();
                    var drivers = row["drivers"].ToString();
                    var laborers = row["laborers"].ToString();
                    var mason = row["mason"].ToString();
                    var carpenter = row["carpenter"].ToString();
                    var material_remarks = row["material_remarks"].ToString();
                    var work_progress = row["work_progress"].ToString();
                    var start_date_time = row["start_date_time"].ToString();
                    var end_date_time = row["end_date_time"].ToString();
                    var updated_on = row["updated_on"].ToString();

                    using (SqlConnection con = new SqlConnection(common.memis()))
                    {
                        SqlCommand com = new SqlCommand();

                        com = new SqlCommand(@"select count(*) from [memis].[dbo].[tblInfraMonitoring] where ppaid = '" + project_id + "'", con);
                        con.Open();

                        int isexist = Convert.ToInt32(com.ExecuteScalar());

                        if (isexist > 0)
                        {
                            com = new SqlCommand($@"update [memis].[dbo].[tblInfraMonitoring] set date_started = '" + date_started + "', date_ended = '" + date_ended + "', time_started =  '" + time_started + "', time_ended = '" + time_ended + "', " +
                            "am_one = '" + am_1 + "', am_two = '" + am_2 + "', am_three = '" + am_3 + "', am_four = '" + am_4 + "', am_five = '" + am_5 + "', am_six = '" + am_6 + "', am_seven = '" + am_7 + "', am_eight = '" + am_8 + "', am_nine = '" + am_9 + "', am_ten = '" + am_10 + "', am_eleven = '" + am_11 + "'," +
                            "am_twelve = '" + am_12 + "', pm_one = '" + pm_1 + "', pm_two = '" + pm_2 + "', pm_three = '" + pm_3 + "', pm_four = '" + pm_4 + "', pm_five = '" + pm_5 + "', pm_six = '" + pm_6 + "', pm_seven = '" + pm_7 + "', pm_eight = '" + pm_8 + "', pm_nine = '" + pm_9 + "', pm_ten = '" + pm_10 + "', " +
                            "pm_eleven = '" + pm_11 + "', pm_twelve = '" + pm_12 + "', project_manager = '" + project_manager + "', project_engineer = '" + project_engineer + "', materials_engineer = '" + materials_engineer + "', safety_engineer = '" + safety_engineer + "', survey_engineer = '" + survey_engineer + "', " +
                            "office_engineer = '" + office_engineer + "', construction_foreman = '" + construction_foreman + "', he_operator = '" + he_operator + "', drivers = '" + drivers + "', laborers = '" + laborers + "', mason = '" + mason + "', carpenter = '" + carpenter + "', material_remarks = '" + material_remarks + "', work_progress = '" + work_progress + "'", con);
                        }
                        else
                        {
                            com = new SqlCommand($@"insert into [memis].[dbo].[tblInfraMonitoring] values ('" + project_id + "','" + title + "','" + location + "','" + date_started + "','" + date_ended + "', '" + time_started + "', '" + time_ended + "', " +
                            "'" + am_1 + "', '" + am_2 + "', '" + am_3 + "', '" + am_4 + "', '" + am_5 + "', '" + am_6 + "', '" + am_7 + "', '" + am_8 + "', '" + am_9 + "', '" + am_10 + "', '" + am_11 + "'," +
                            "'" + am_12 + "', '" + pm_1 + "', '" + pm_2 + "', '" + pm_3 + "', '" + pm_4 + "', '" + pm_5 + "', '" + pm_6 + "', '" + pm_7 + "', '" + pm_8 + "', '" + pm_9 + "', '" + pm_10 + "', '" + pm_11 + "', '" + pm_12 + "', '" + project_manager + "'," +
                            "'" + project_engineer + "', '" + materials_engineer + "', '" + safety_engineer + "', '" + survey_engineer + "','" + office_engineer + "', '" + construction_foreman + "', '" + he_operator + "', '" + drivers + "', '" + laborers + "'," +
                            "'" + mason + "', '" + carpenter + "', '" + material_remarks + "', '" + work_progress + "','" + EID + "')", con);
                        }

                        var spms_is_exist = (@"select count(*) from [spms].[dbo].[spms_tblSubTask] where project_id = '" + project_id + "' and eid = '"+ EID +"'").Scalar();

                        if (spms_is_exist > 0)
                        {
                            ret = "succes";
                            return ret;
                        }
                        else
                        {
                            QRY = @"insert into [spms].[dbo].[spms_tblSubTask]  values(0,'" + project_id + "','" + title.Replace("'", "''") + "','" + date_started + "','" + date_ended + "','" + time_started + "','" + time_ended + "','" + EID + "',1,0,'" + updated_on + "','" + date_started + "','" + date_ended + "','" + time_started + "','" + time_ended + "','" + start_date_time + "','" + end_date_time + "',0,0,0,'" + material_remarks + "',1,1,0,0) select SCOPE_IDENTITY();";

                            s_id = (QRY).Scalar();
                        }
                         

                        //con.Open();
                        //com.ExecuteNonQuery();
                        SqlDataReader reader = com.ExecuteReader();
                        con.Close();

                    }

                    DataTable GetPicture = dtattach.Clone();
                    DataRow[] select_pic = dtattach.Select("ppaid = '" + project_id + "'");

                    int count = 0;
                    foreach (DataRow pic_row in select_pic)
                    {
                        count++;
                        GetPicture.ImportRow(pic_row);
                    }

                    if (count > 0)
                    {
                        foreach (DataRow r in GetPicture.Rows)
                        {
                            //String qry = "";
                            String PIC_QRY = "";
                            var id = r["id"].ToString();
                            var picture = r["picture"].ToString();
                            var date_entry = r["date_entry"].ToString();
                            var ppaid = r["ppaid"].ToString();
                            var position_name = r["position_name"].ToString();
                            var longitude = r["longitude"].ToString();
                            var latitude = r["latitude"].ToString();
                            var mill = r["mill"].ToString();

                            /*using (SqlConnection con = new SqlConnection(common.memis()))
                            {
                                SqlCommand com = new SqlCommand();

                                com = new SqlCommand($@"insert into [memis].[dbo].[tblInfraAttachment] values('','" + date_entry + "','" + ppaid + "','" + position_name + "','" + longitude + "','" + latitude + "','" + EID + "','png','" + mill + "')", con);

                                con.Open();
                                com.ExecuteNonQuery();
                                //dt.Load(reader);
                                con.Close();
                            }*/


                            PIC_QRY = @"insert into [spms].[dbo].[spms_tblSubTaskProof] values('','" + date_entry + "','" + s_id + "','" + longitude + "','" + latitude + "',' ','" + EID + "','png','" + mill + "') select SCOPE_IDENTITY();";


                            var pic_id = (PIC_QRY).Scalar();

                            (@"update [spms].[dbo].[spms_tblSubTaskProof] set attachment = '" + pic_id + "' where id = '" + pic_id + "'").NonQuery();
                             

                            byte[] imagearr = Convert.FromBase64String(picture);
                            MemoryStream ms = new MemoryStream(imagearr, 0, imagearr.Length);
                            ms.Write(imagearr, 0, imagearr.Length);

                            using (new ConnectToSharedFolder(networkPath, credentials))
                            {
                                try
                                {
                                    var newpath = networkPath + "\\" + EID;
                                    if (!Directory.Exists(newpath))
                                    {
                                        Directory.CreateDirectory(newpath);
                                    }

                                    Image img = Image.FromStream(ms, true, true);
                                    ReduceImageSizeAndSave(newpath + @"\" + pic_id + ".png", img);


                                }
                                catch (Exception ex)
                                {

                                }
                            }

                            try
                            {
                                String qry = @"insert into [spms].[dbo].[tbl_infra_spms_logs]  values ('" + s_id + "','" + pic_id + "')";
                                (qry).NonQuery();
                            }
                            catch (Exception ex)
                            {

                            }


                        }

                    }



                    ret = "success";

                }

            }
            catch (Exception ex)
            {
                ret = "failed";
            }

            try
            {
                dtp.Columns.Add("ppaid");
                dtp.Columns.Add("equipment");
                dtp.Columns.Add("qty");
                dtp.Columns.Add("operating");
                dtp.Columns.Add("standby");
                dtp.Columns.Add("breakdown"); 

                foreach (var s in ie.OfType<infra_equipment>())
                {
                    dtp.Rows.Add(s.ppaid, s.equipment, s.qty, s.operating, s.standby, s.breakdown);
                }

                foreach (DataRow r in dtp.Rows)
                {
                    //String qry = "";
                    var ppaid = r["ppaid"].ToString();
                    var equipment = r["equipment"].ToString();
                    var qty = r["qty"].ToString();
                    var operating = r["operating"].ToString();
                    var standby = r["standby"].ToString();
                    var breakdown = r["breakdown"].ToString(); 


                    using (SqlConnection con = new SqlConnection(common.memis()))
                    {
                        SqlCommand com = new SqlCommand();


                        //com = new SqlCommand(@"select count(*) from[property].[dbo].[tbl_NewProperty] where other_description = '" + otherdescription + "' and tablet_id = '" + androidid + "'", con);
                        //con.Open();

                        //int isexist = Convert.ToInt32(com.ExecuteScalar());

                        if (operating == "true")
                        {
                            com = new SqlCommand($@"insert into [memis].[dbo].[tblInfraEquipment] values ('" + ppaid + "', '" + equipment + "', '" + qty + "', '1', '0', '0')", con);
                        }
                        else if (standby == "true")
                        {
                            com = new SqlCommand($@"insert into [memis].[dbo].[tblInfraEquipment] values ('" + ppaid + "', '" + equipment + "', '" + qty + "', '0', '1', '0')", con);
                        }
                        else if (breakdown == "true")
                        {
                            com = new SqlCommand($@"insert into [memis].[dbo].[tblInfraEquipment] values ('" + ppaid + "', '" + equipment + "', '" + qty + "', '0', '0', '1')", con);
                        }

                        con.Open();
                        com.ExecuteNonQuery();
                        //dt.Load(reader);
                        con.Close();
                    }


                    ret = "success";

                }
            }
            catch (Exception ex)
            {

            }

            try
            {
                dtm.Columns.Add("ppaid");
                dtm.Columns.Add("location");
                dtm.Columns.Add("description");
                dtm.Columns.Add("qty");
                dtm.Columns.Add("unit");
                dtm.Columns.Add("accepted");
                dtm.Columns.Add("rejected");

                foreach (var s in im.OfType<infra_materials>())
                {
                    dtm.Rows.Add(s.ppaid, s.location, s.description, s.qty, s.unit, s.accepted, s.rejected);
                }

                foreach (DataRow r in dtm.Rows)
                {
                    //String qry = "";
                    var ppaid = r["ppaid"].ToString();
                    var location = r["location"].ToString();
                    var description = r["description"].ToString();
                    var qty = r["qty"].ToString();
                    var unit = r["unit"].ToString();
                    var accepted = r["accepted"].ToString();
                    var rejected = r["rejected"].ToString();


                    using (SqlConnection con = new SqlConnection(common.memis()))
                    {
                        SqlCommand com = new SqlCommand();


                        //com = new SqlCommand(@"select count(*) from[property].[dbo].[tbl_NewProperty] where other_description = '" + otherdescription + "' and tablet_id = '" + androidid + "'", con);
                        //con.Open();

                        //int isexist = Convert.ToInt32(com.ExecuteScalar());

                        if (accepted == "true")
                        {
                            com = new SqlCommand($@"insert into [memis].[dbo].[tblInfraMaterials] values ('" + ppaid + "', '" + location + "', '" + description + "', '" + qty + "', '" + unit + "', '1', '0')", con);
                        }
                        else if (rejected == "true")
                        {
                            com = new SqlCommand($@"insert into [memis].[dbo].[tblInfraMaterials] values ('" + ppaid + "', '" + location + "', '" + description + "', '" + qty + "', '" + unit + "', '0', '1')", con);
                        }
                        

                        con.Open();
                        com.ExecuteNonQuery();
                        //dt.Load(reader);
                        con.Close();
                    }


                    ret = "success";

                }
            }
            catch (Exception ex)
            {

            }

            try
            {
                dtf.Columns.Add("ppaid");
                dtf.Columns.Add("findings");
                dtf.Columns.Add("recommendations"); 

                foreach (var s in findings.OfType<infra_findings>())
                {
                    dtf.Rows.Add(s.ppaid, s.findings, s.recommendations);
                }

                foreach (DataRow r in dtf.Rows)
                {
                    //String qry = "";
                    var ppaid = r["ppaid"].ToString();
                    var findings1 = r["findings"].ToString();
                    var recommendations = r["recommendations"].ToString(); 


                    using (SqlConnection con = new SqlConnection(common.memis()))
                    {
                        SqlCommand com = new SqlCommand();

                        com = new SqlCommand($@"insert into [memis].[dbo].[tblInfaFindings] values ('" + ppaid + "', '" + findings1 + "', '" + recommendations + "')", con);

                        con.Open();
                        com.ExecuteNonQuery();
                        //dt.Load(reader);
                        con.Close();
                    }


                    ret = "success";

                }
            }
            catch (Exception ex)
            {

            }

            /*try
            {
                dtattach.Columns.Add("id");
                dtattach.Columns.Add("picture");
                dtattach.Columns.Add("date_entry");
                dtattach.Columns.Add("ppaid");
                dtattach.Columns.Add("position_name");
                dtattach.Columns.Add("longitude");
                dtattach.Columns.Add("latitude");
                //dtattach.Columns.Add("eid");
                dtattach.Columns.Add("mill");

                foreach (var s in attach.OfType<infra_attachment>())
                {
                    dtattach.Rows.Add(s.id, s.picture, s.date_entry, s.ppaid, s.position_name, s.longitude, s.latitude, s.mill);
                }

                foreach (DataRow r in dtattach.Rows)
                {
                    //String qry = "";
                    String PIC_QRY = "";
                    var id = r["id"].ToString();
                    var picture = r["picture"].ToString();
                    var date_entry = r["date_entry"].ToString();
                    var ppaid = r["ppaid"].ToString();
                    var position_name = r["position_name"].ToString();
                    var longitude = r["longitude"].ToString();
                    var latitude = r["latitude"].ToString();
                    //var eid = r["eid"].ToString();
                    var mill = r["mill"].ToString();

                    *//*using (SqlConnection con = new SqlConnection(common.memis()))
                    {
                        SqlCommand com = new SqlCommand();

                        com = new SqlCommand($@"insert into [memis].[dbo].[tblInfraAttachment] values('','" + date_entry + "','" + ppaid + "','" + position_name + "','" + longitude + "','" + latitude + "','" + EID + "','png','" + mill + "')", con);

                        con.Open();
                        com.ExecuteNonQuery();
                        //dt.Load(reader);
                        con.Close();
                    }*//*
                    

                   

                    PIC_QRY = @"insert into [spms].[dbo].[spms_tblSubTaskProof] values('','" + date_entry + "','" + s_id + "','" + r_longitude + "','" + r_latitude + "','" + r_status + "','" + EID + "','png','" + r_mill + "') select SCOPE_IDENTITY();";


                    var pic_id = (PIC_QRY).Scalar();

                    (@"update [spms].[dbo].[spms_tblSubTaskProof] set attachment = '" + pic_id + "' where id = '" + pic_id + "'").NonQuery();

                    int s_id = (QRY).Scalar();
                    try
                    {
                        String qry = @"insert into [spms].[dbo].[tbl_infra_spms_logs]  values ('" + s_id + "',1)";
                        (qry).NonQuery();
                    }
                    catch (Exception ex)
                    {

                    }


                    byte[] imagearr = Convert.FromBase64String(picture);
                    MemoryStream ms = new MemoryStream(imagearr, 0, imagearr.Length);
                    ms.Write(imagearr, 0, imagearr.Length);

                    using (new ConnectToSharedFolder(networkPath, credentials))
                    {
                        try
                        {
                            var newpath = networkPath + "\\" + EID;
                            if (!Directory.Exists(newpath))
                            {
                                Directory.CreateDirectory(newpath);
                            }

                            Image img = Image.FromStream(ms, true, true);
                            ReduceImageSizeAndSave(newpath + @"\" + picture + ".png", img);


                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    ret = "success";

                }
            }
            catch (Exception ex)
            {

            }*/

            return "1";
        }
         

        [System.Web.Services.WebMethod()]
        public string SaveActivity(string StartDateString, string EndDateString, string StartTime, string EndTime,
                                    int TypeID, string EmployeeIDs, int LocationID, string Description, string dtsdocID, int eid)
        {
            String r1 = EmployeeIDs.Replace("[", "");
            String r2 = r1.Replace("]", "");
            if (dtsdocID == "0")
            {
                dtsdocID = "";
            }

            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"spms_sp_InsertIntoCalendarofActivities '" + r2 + "','" + StartDateString + "','" + EndDateString
                        + "','" + StartTime + "','" + EndTime + "'," + Convert.ToInt32(TypeID) + "," + Convert.ToInt32(LocationID) + ",'" + Description + "'," + Convert.ToInt64(eid) + ",'" + dtsdocID  + "'", con);
                    con.Open();
                    var result = query.ExecuteScalar().ToString();
                    SendEmail(StartDateString, EndDateString, StartTime, EndTime, Convert.ToInt32(TypeID), r2, Convert.ToInt32(LocationID), Description, result.Split(new string[] { "$3xhPsd@rxcA2r" }, StringSplitOptions.None)[1], dtsdocID, Convert.ToInt32(eid));
                   //return result.Split(new string[] { "$3xhPsd@rxcA2r" }, StringSplitOptions.None)[0];
                    return "1";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        
        [System.Web.Services.WebMethod()]
        public string UpdateActivity(string StartDateString, string EndDateString, string StartTime, string EndTime,
                                    int TypeID, string EmployeeIDs, int LocationID, string Description, string dtsdocID, int DataID,int eid_param)
        {
            String r1 = EmployeeIDs.Replace("[", "");
            String r2 = r1.Replace("]", "");

            if (dtsdocID == "0")
            {
                dtsdocID = "";
            }

            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"select top 1 a.id,a.event_type_id,a.location_id,a.description,
                                                   cast(format(a.start_date,'MM/dd/yyyy') as varchar(max)) + '::sep::' + CONVERT(VARCHAR,a.start_time,100),
                                                   cast(format(a.end_date,'MM/dd/yyyy') as varchar(max)) + '::sep::' + CONVERT(VARCHAR,a.end_time,100),doc_id,
                                                   eids = STUFF((
                                                                SELECT ',' + cast(eid as varchar(max))
                                                                FROM spms_tblMeetingParticipants
			                                                    where Meeting_id = a.id
                                                                FOR XML PATH('')
                                                           ), 1, 1, '')
			                                        from spms_tblCalendarOfActivities as a
			                                        where id = " + DataID + "", con);
                    con.Open();
                    SqlDataReader reader = query.ExecuteReader();
                    reader.Read();
                    SendMail_UpdatedEventDetails(reader.GetValue(4).ToString().Split(new string[] { "::sep::" }, StringSplitOptions.None)[0], reader.GetValue(4).ToString().Split(new string[] { "::sep::" }, StringSplitOptions.None)[0], reader.GetValue(4).ToString().Split(new string[] { "::sep::" }, StringSplitOptions.None)[1], reader.GetValue(5).ToString().Split(new string[] { "::sep::" }, StringSplitOptions.None)[1], Convert.ToInt32(reader.GetValue(1)), reader.GetValue(7).ToString(), Convert.ToInt32(2), reader.GetValue(3).ToString(), reader.GetValue(6).ToString(),eid_param);
                }
                //Delete the old Schedule and Participants
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"delete from spms_tblCalendarOfActivities where id = " + DataID +
                                                        "delete from spms_tblMeetingParticipants where Meeting_id = " + DataID + "", con);
                    con.Open();
                    query.ExecuteNonQuery();
                }
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"spms_sp_InsertIntoCalendarofActivities '" + r2 + "','" + StartDateString + "','" + EndDateString
                        + "','" + StartTime + "','" + EndTime + "'," + TypeID + "," + LocationID + ",'" + Description + "'," + eid_param + ",'" + dtsdocID + "'", con);
                    con.Open();
                    var result = query.ExecuteScalar().ToString();
                    SendEmail(StartDateString, EndDateString, StartTime, EndTime, TypeID, r2, LocationID, Description, result.Split(new string[] { "$3xhPsd@rxcA2r" }, StringSplitOptions.None)[1], dtsdocID, eid_param);
                   // return result.Split(new string[] { "$3xhPsd@rxcA2r" }, StringSplitOptions.None)[0];
                    return "1";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [System.Web.Services.WebMethod()]
        public void SendMail_UpdatedEventDetails(string StartDateString, string EndDateString, string StartTime, string EndTime,
                                    int TypeID, string EmployeeIDs, int LocationID, string Description, string dtsdocID, int eid_param)
        {
            if (dtsdocID == "0")
            {
                dtsdocID = "";
            }

            var EmployeeIDList = EmployeeIDs.Split(',');
            var EmployeeName = "";
            var Location = "";
            var Type = "";
            var EmailAddress = "";
            var SenderName = "";
            var AttachmentLink = "";
            if (TypeID == 1)
            {
                Type = "Meeting";
            }
            else if (TypeID == 2)
            {
                Type = "Event";
            }
            else
            {
                Type = "Holiday";
            }
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select top 1 Description from spms_tblVenue where id = " + LocationID + "", con);
                con.Open();
                Location = query.ExecuteScalar().ToString();
            }
            if (dtsdocID != "")
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"select 'http://192.168.2.104/dts/public/read?docid=' + cast(a.docid as varchar(100)) + '&passkey=' + b.filecode  
                                                    from dts.dbo.t_doc_vw as a
                                                    left join dts.dbo.t_doc_file_vw as b on b.docid = a.docid
                                                     where  a.docid = " + dtsdocID + "", con);
                    con.Open();
                    AttachmentLink = query.ExecuteScalar().ToString();
                }
            }
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select case when sex = 'Male' then 'Mr. ' 
				       when sex = 'Female' and CivilStatus = 'Single' then 'Ms. ' 
					   when sex = 'Female' and CivilStatus = 'Married' then 'Mrs. ' 
					   when sex = 'Female' and CivilStatus = 'Widowed' then 'Mrs. ' else '' end + UPPER(LEFT(b.Firstname,1))+LOWER(SUBSTRING(b.Firstname,2,LEN(b.Firstname))) + ' ' + UPPER(LEFT(b.Lastname,1))+LOWER(SUBSTRING(b.Lastname,2,LEN(b.Lastname))) from pmis.dbo.eportalUser as a
                        left join pmis.dbo.employee as b on b.eid= a.eid where a.eid = " + eid_param + "", con);
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                reader.Read();
                SenderName = reader.GetValue(0).ToString();
            }
            foreach (var eid in EmployeeIDList)
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"select a.username,case when sex = 'Male' then 'Mr. ' 
				       when sex = 'Female' and CivilStatus = 'Single' then 'Ms. ' 
					   when sex = 'Female' and CivilStatus = 'Married' then 'Mrs. ' 
					   when sex = 'Female' and CivilStatus = 'Widowed' then 'Mrs. ' else '' end + UPPER(LEFT(b.Firstname,1))+LOWER(SUBSTRING(b.Firstname,2,LEN(b.Firstname))) + ' ' + UPPER(LEFT(b.Lastname,1))+LOWER(SUBSTRING(b.Lastname,2,LEN(b.Lastname))) from pmis.dbo.eportalUser as a
                        left join pmis.dbo.employee as b on b.eid= a.eid where a.eid = " + eid + "", con);
                    con.Open();
                    SqlDataReader reader = query.ExecuteReader();
                    reader.Read();
                    EmailAddress = reader.GetValue(0).ToString();
                    EmployeeName = reader.GetValue(1).ToString();
                }

                MailMessage message = new MailMessage();
                message.From = new MailAddress("spms@pgas.gov", "SPMS");

                message.To.Add(new MailAddress(EmailAddress));

                message.Subject = Type;

                message.Body = @"Good Day! " + EmployeeName + "</br></br> I would like to Inform you that the " + Type.ToLower() + " on <b> " + Description + "</b> to be held at <b>" + Location + "</b> dated <b>" + StartDateString.Replace("-", "/") + " " + StartTime + " - " + EndDateString.Replace("-", "/") + " " + EndTime + "</b> has been updated. </br></br> We will send a new email regarding with the new details of that said " + Type.ToLower() + "; If you didn't recieve the new details of that " + Type.ToLower() + ", you're already not part of the list of participants of the said " + Type.ToLower() + ". </br></br></br></br>For more information please contact " + SenderName + "</br></br> Note: <i>this is a system generated email please don't reply.</i>";

                message.IsBodyHtml = true;


                SmtpClient client = new SmtpClient();
                client.Host = "192.168.2.101";
                client.Send(message);
            }
        }

        
        [System.Web.Services.WebMethod()]
        public void SendEmail(string StartDateString, string EndDateString, string StartTime, string EndTime,
                                    int TypeID, string EmployeeIDs, int LocationID, string Description, string MeetingID, string dtsdocID,int eid_param)
        {
            var EmployeeIDList = EmployeeIDs.Split(',');
            var EmployeeName = "";
            var Location = "";
            var Type = "";
            var EmailAddress = "";
            var Parameter = "";
            var SenderName = "";
            var AttachmentLink = "";
            if (TypeID == 1)
            {
                Type = "Meeting";
            }
            else if (TypeID == 2)
            {
                Type = "Event";
            }
            else
            {
                Type = "Holiday";
            }
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select top 1 Description from spms_tblVenue where id = " + LocationID + "", con);
                con.Open();
                Location = query.ExecuteScalar().ToString();
            }
            if (dtsdocID != "")
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"select 'http://192.168.2.104/dts/public/read?docid=' + cast(a.docid as varchar(100)) + '&passkey=' + b.filecode  
                                                    from dts.dbo.t_doc_vw as a
                                                    left join dts.dbo.t_doc_file_vw as b on b.docid = a.docid
                                                     where  a.docid = " + dtsdocID + "", con);
                    con.Open();
                    AttachmentLink = query.ExecuteScalar().ToString();
                }
            }

            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select case when sex = 'Male' then 'Mr. ' 
				       when sex = 'Female' and CivilStatus = 'Single' then 'Ms. ' 
					   when sex = 'Female' and CivilStatus = 'Married' then 'Mrs. ' 
					   when sex = 'Female' and CivilStatus = 'Widowed' then 'Mrs. ' else '' end + UPPER(LEFT(b.Firstname,1))+LOWER(SUBSTRING(b.Firstname,2,LEN(b.Firstname))) + ' ' + UPPER(LEFT(b.Lastname,1))+LOWER(SUBSTRING(b.Lastname,2,LEN(b.Lastname))) from pmis.dbo.eportalUser as a
                        left join pmis.dbo.employee as b on b.eid= a.eid where a.eid = " + eid_param + "", con);
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                reader.Read();
                SenderName = reader.GetValue(0).ToString();
            }

            foreach (var eid in EmployeeIDList)
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"select a.username,case when sex = 'Male' then 'Mr. ' 
				       when sex = 'Female' and CivilStatus = 'Single' then 'Ms. ' 
					   when sex = 'Female' and CivilStatus = 'Married' then 'Mrs. ' 
					   when sex = 'Female' and CivilStatus = 'Widowed' then 'Mrs. ' else '' end + UPPER(LEFT(b.Firstname,1))+LOWER(SUBSTRING(b.Firstname,2,LEN(b.Firstname))) + ' ' + UPPER(LEFT(b.Lastname,1))+LOWER(SUBSTRING(b.Lastname,2,LEN(b.Lastname))) from pmis.dbo.eportalUser as a
                        left join pmis.dbo.employee as b on b.eid= a.eid where a.eid = " + eid + "", con);
                    con.Open();
                    SqlDataReader reader = query.ExecuteReader();
                    reader.Read();
                    EmailAddress = reader.GetValue(0).ToString();
                    EmployeeName = reader.GetValue(1).ToString();
                }

                Parameter = eid.ToString() + "," + MeetingID.ToString() + ",";
                var buttons = @"<div><a style='color:white;border:solid #1f8c35 1px;background-color:#62cb31;border-radius: 28px;padding:1px 1px 1px 1px' href='http://192.168.2.104/spms/MailResponse/?Param=" + Server.UrlEncode(Rijndael.Encrypt(Parameter + "1")) + "'>&nbsp;Count me in!&nbsp;</a>&nbsp;&nbsp;<a style='color:white;border:solid #1f8c35 1px;background-color:#38bbe2;border-radius: 28px;padding:1px 1px 1px 1px;'             href='http://192.168.2.104/spms/MailResponse/?Param=" + Server.UrlEncode(Rijndael.Encrypt(Parameter + "2")) + "'>&nbsp;Send Someone!&nbsp;</a>&nbsp;&nbsp;<a style='color:white;border:solid #1f8c35 1px;background-color:#ea2525;border-radius: 28px;padding:1px 1px 1px 1px;margin:left:10px' href='http://192.168.2.104/spms/MailResponse/?Param=" + Server.UrlEncode(Rijndael.Encrypt(Parameter + "4")) + "'>&nbsp;Expect me bringing someone!&nbsp;</a></div>";
                MailMessage message = new MailMessage();
                message.From = new MailAddress("spms@pgas.gov", "SPMS");

                message.To.Add(new MailAddress(EmailAddress));

                message.Subject = Type;

                message.Body = @"Good Day! " + EmployeeName + "</br></br> You have been invited as one of the attendees on <b>" + Description + "</b> to be held at <b>" + Location + "</b> dated <b>" + StartDateString.Replace("-", "/") + " " + StartTime + " - " + EndDateString.Replace("-", "/") + " " + EndTime + "</b> </br></br> Click on the button below to respond. </br></br>" + buttons + " " + (dtsdocID == "" ? "" : "</br></br><a href='" + AttachmentLink + "'>CLICK HERE TO VIEW ATTACHMENT</a>") + "</br></br>For more information please contact " + SenderName + "</br></br> Note: <i>this is a system generated email please don't reply.</i>";

                message.IsBodyHtml = true;

                //System.Net.Mail.Attachment attachment;
                //attachment = new System.Net.Mail.Attachment("D:\\Pag-ibig Loyalty card Form.pdf");
                //message.Attachments.Add(attachment);

                SmtpClient client = new SmtpClient();
                client.Host = "192.168.2.101";
                client.Send(message);
            }
        }

        
        [System.Web.Services.WebMethod()]
        public string DeleteEvent(int EventID, string Reason,int eid_param)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"delete from spms_tblCalendarofActivities where id = " + EventID + "", con);
                    con.Open();
                    SendEmailCanceledEvent(EventID, Reason, eid_param);
                    query.ExecuteNonQuery();
                    return "1";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        
        [System.Web.Services.WebMethod()]
        public string UpdateEvent_Drag(int EventID, DateTime NewStartDateString, DateTime NewEndDateString, int SendMail, int eid_param)
        {
            var StartDate = NewStartDateString.ToString("yyyy-MM-dd");
            var StartTime = NewStartDateString.ToString("hh:mm tt");
            var EndDate = NewEndDateString.ToString("yyyy-MM-dd");
            var EndTime = NewEndDateString.ToString("hh:mm tt");
            try
            {

                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"update spms_tblCalendarofActivities set start_date = '" + StartDate + "',End_date = '" + EndDate + "',start_Time = '" + StartTime + "',end_time = '" + EndTime + "'  where id = " + EventID + "", con);
                    con.Open();
                    if (SendMail == 1)
                    {
                        SendEmailUpdatedEvent(EventID, NewStartDateString, NewEndDateString,eid_param);
                    }
                    query.ExecuteNonQuery();
                    return "1";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        
        [System.Web.Services.WebMethod()]
        public void SendEmailCanceledEvent(int EventID, string Reason, int eid_param)
        {
            var EmployeeName = "";
            var Location = "";
            var Type = "";
            var EmailAddress = "";
            var StartDateString = "";
            var EndDateString = "";
            var StartTime = "";
            var EndTime = "";
            var Description = "";
            var SenderName = "";
            var EmployeeIDs = "";
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select case when a.event_type_id = 1 then 'Meeting' when a.event_type_id = 2 then 'Event' else 'Holiday' end as EventType,a.description,format(a.start_date,'yyyy-dd-MM'),format(a.end_date,'yyyy-dd-MM'), 
                  CONVERT(varchar(15),CAST(start_time AS TIME),100),
                  CONVERT(varchar(15),CAST(end_time AS TIME),100),b.Description
                from spms_tblCalendarOfActivities as a
                left join spms_tblVenue as b on b.id = a.location_id
                 where a.id = " + EventID + "", con);
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                reader.Read();
                Type = reader.GetValue(0).ToString();
                Description = reader.GetValue(1).ToString();
                StartDateString = reader.GetValue(2).ToString();
                EndDateString = reader.GetValue(3).ToString();
                StartTime = reader.GetValue(4).ToString().Replace("A", " A").Replace("P", " P");
                EndTime = reader.GetValue(5).ToString().Replace("A", " A").Replace("P", " P");
                Location = reader.GetValue(6).ToString();
            }
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select eid from spms_tblMeetingParticipants where Meeting_id = " + EventID + " and (can_participate != 0 or can_participate is null)", con);
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    EmployeeIDs += EmployeeIDs == "" ? reader.GetValue(0).ToString() : "," + reader.GetValue(0).ToString();
                }
            }
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select case when sex = 'Male' then 'Mr. ' 
				       when sex = 'Female' and CivilStatus = 'Single' then 'Ms. ' 
					   when sex = 'Female' and CivilStatus = 'Married' then 'Mrs. ' 
					   when sex = 'Female' and CivilStatus = 'Widowed' then 'Mrs. ' else '' end + UPPER(LEFT(b.Firstname,1))+LOWER(SUBSTRING(b.Firstname,2,LEN(b.Firstname))) + ' ' + UPPER(LEFT(b.Lastname,1))+LOWER(SUBSTRING(b.Lastname,2,LEN(b.Lastname))) from pmis.dbo.eportalUser as a
                        left join pmis.dbo.employee as b on b.eid= a.eid where a.eid = " + eid_param + "", con);
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                reader.Read();
                SenderName = reader.GetValue(0).ToString();
            }
            foreach (var eid in EmployeeIDs.Split(','))
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"select a.username,case when sex = 'Male' then 'Mr. ' 
				       when sex = 'Female' and CivilStatus = 'Single' then 'Ms. ' 
					   when sex = 'Female' and CivilStatus = 'Married' then 'Mrs. ' 
					   when sex = 'Female' and CivilStatus = 'Widowed' then 'Mrs. ' else '' end + UPPER(LEFT(b.Firstname,1))+LOWER(SUBSTRING(b.Firstname,2,LEN(b.Firstname))) + ' ' + UPPER(LEFT(b.Lastname,1))+LOWER(SUBSTRING(b.Lastname,2,LEN(b.Lastname))) from pmis.dbo.eportalUser as a
                        left join pmis.dbo.employee as b on b.eid= a.eid where a.eid = " + eid + "", con);
                    con.Open();
                    SqlDataReader reader = query.ExecuteReader();
                    reader.Read();
                    EmailAddress = reader.GetValue(0).ToString();
                    EmployeeName = reader.GetValue(1).ToString();
                }

                MailMessage message = new MailMessage();
                message.From = new MailAddress("spms@pgas.gov", "SPMS");

                message.To.Add(new MailAddress(EmailAddress));

                message.Subject = Type + " Cancellation";

                message.Body = @"Good Day! " + EmployeeName + "</br></br>This is to inform you that the " + Type + " on <b>" + Description + "</b> to be held at <b>" + Location + "</b> dated <b>" + StartDateString.Replace("-", "/") + " " + StartTime + " - " + EndDateString.Replace("-", "/") + " " + EndTime + "</b> has been cancelled. " + (Reason == "" ? "" : " Due to " + Reason) + "</br></br>For more information please ask " + SenderName + "</br></br> Note: <i>this is a system generated email please don't reply.</i>";

                message.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();
                client.Host = "192.168.2.101";
                client.Send(message);
            }
        }

        [System.Web.Services.WebMethod()]
        public void SendEmailUpdatedEvent(int EventID, DateTime NewStartDateString, DateTime NewEndDateString,int eid_param)
        {
            var EmployeeName = "";
            var Location = "";
            var Type = "";
            var EmailAddress = "";
            var StartDateString = "";
            var EndDateString = "";
            var StartTime = "";
            var EndTime = "";
            var Description = "";
            var SenderName = "";
            var EmployeeIDs = "";

            var NewStartDate = NewStartDateString.ToString("yyyy/dd/MM");
            var NewStartTime = NewStartDateString.ToString("hh:mm tt");
            var NewEndDate = NewEndDateString.ToString("yyyy/dd/MM");
            var NewEndTime = NewEndDateString.ToString("hh:mm tt");
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select case when a.event_type_id = 1 then 'Meeting' when a.event_type_id = 2 then 'Event' else 'Holiday' end as EventType,a.description,format(a.start_date,'yyyy-dd-MM'),format(a.end_date,'yyyy-dd-MM'), 
                  CONVERT(varchar(15),CAST(start_time AS TIME),100),
                  CONVERT(varchar(15),CAST(end_time AS TIME),100),b.Description
                from spms_tblCalendarOfActivities as a
                left join spms_tblVenue as b on b.id = a.location_id
                 where a.id = " + EventID + "", con);
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                reader.Read();
                Type = reader.GetValue(0).ToString();
                Description = reader.GetValue(1).ToString();
                StartDateString = reader.GetValue(2).ToString();
                EndDateString = reader.GetValue(3).ToString();
                StartTime = reader.GetValue(4).ToString().Replace("A", " A").Replace("P", " P");
                EndTime = reader.GetValue(5).ToString().Replace("A", " A").Replace("P", " P");
                Location = reader.GetValue(6).ToString();
            }
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select eid from spms_tblMeetingParticipants where Meeting_id = " + EventID + " and (can_participate != 0 or can_participate is null)", con);
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    EmployeeIDs += EmployeeIDs == "" ? reader.GetValue(0).ToString() : "," + reader.GetValue(0).ToString();
                }
            }
            using (SqlConnection con = new SqlConnection(common.MyConnection()))
            {
                SqlCommand query = new SqlCommand(@"select case when sex = 'Male' then 'Mr. ' 
				       when sex = 'Female' and CivilStatus = 'Single' then 'Ms. ' 
					   when sex = 'Female' and CivilStatus = 'Married' then 'Mrs. ' 
					   when sex = 'Female' and CivilStatus = 'Widowed' then 'Mrs. ' else '' end + UPPER(LEFT(b.Firstname,1))+LOWER(SUBSTRING(b.Firstname,2,LEN(b.Firstname))) + ' ' + UPPER(LEFT(b.Lastname,1))+LOWER(SUBSTRING(b.Lastname,2,LEN(b.Lastname))) from pmis.dbo.eportalUser as a
                        left join pmis.dbo.employee as b on b.eid= a.eid where a.eid = " + eid_param + "", con);
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                reader.Read();
                SenderName = reader.GetValue(0).ToString();
            }
            foreach (var eid in EmployeeIDs.Split(','))
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand query = new SqlCommand(@"select a.username,case when sex = 'Male' then 'Mr. ' 
				       when sex = 'Female' and CivilStatus = 'Single' then 'Ms. ' 
					   when sex = 'Female' and CivilStatus = 'Married' then 'Mrs. ' 
					   when sex = 'Female' and CivilStatus = 'Widowed' then 'Mrs. ' else '' end + UPPER(LEFT(b.Firstname,1))+LOWER(SUBSTRING(b.Firstname,2,LEN(b.Firstname))) + ' ' + UPPER(LEFT(b.Lastname,1))+LOWER(SUBSTRING(b.Lastname,2,LEN(b.Lastname))) from pmis.dbo.eportalUser as a
                        left join pmis.dbo.employee as b on b.eid= a.eid where a.eid = " + eid + "", con);
                    con.Open();
                    SqlDataReader reader = query.ExecuteReader();
                    reader.Read();
                    EmailAddress = reader.GetValue(0).ToString();
                    EmployeeName = reader.GetValue(1).ToString();
                }

                MailMessage message = new MailMessage();
                message.From = new MailAddress("spms@pgas.gov", "SPMS");

                message.To.Add(new MailAddress(EmailAddress));

                message.Subject = Type + " Schedule Update";

                message.Body = @"Good Day! " + EmployeeName + "</br></br>This is to inform you that the schedule on the " + Type + " on <b>" + Description + "</b> to be held at <b>" + Location + "</b> dated <b>" + StartDateString.Replace("-", "/") + " " + StartTime + " - " + EndDateString.Replace("-", "/") + " " + EndTime + "</b> has been Moved. </br></br> the new schedule will be on <b>" + NewStartDate.Replace("-", "/") + " " + NewStartTime + " - " + NewEndDate.Replace("-", "/") + " " + NewEndTime + "</b> </br></br>For more information please ask " + SenderName + "</br></br> Note: <i>this is a system generated email please don't reply.</i>";

                message.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();
                client.Host = "192.168.2.101";
                client.Send(message);
            }
        }

        [System.Web.Services.WebMethod()]
        public DataTable get_location()
        {
            string qry = @"select id,Description from spms_tblVenue";
            DataTable dt = new DataTable("locations");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(qry, con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Services.WebMethod()]
        public DataTable get_document(int eid)
        {

            int officeid = (@"select OfficeID from spms_tblOrganizationalChart where eid = " + eid).Scalar();

            string qry = @"select docid,title from dts.dbo.t_doc_vw where officeid = "+officeid+"";
            DataTable dt = new DataTable("document");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(qry, con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [System.Web.Services.WebMethod()]
        public DataTable getLocationDocumentText(int meetingId)
        {
           
            string qry = @" select  a.*,b.*  from (select a.id,b.title,a.location_id,a.doc_id,c.Description from [spms].[dbo].[spms_tblCalendarOfActivities] as a left join dts.dbo.t_doc_vw as b on a.doc_id = b.docid inner join [spms].[dbo].[spms_tblVenue] as c on a.location_id = c.id where a.id = " + meetingId + ") as a inner join ( SELECT Meeting_id,eids = STUFF((SELECT ',' + Cast(eid as nvarchar) FROM [spms].[dbo].[spms_tblMeetingParticipants]  t1 WHERE t1.Meeting_id = t2.Meeting_id FOR XML PATH ('')), 1, 1, '') from [spms].[dbo].[spms_tblMeetingParticipants]  t2 where Meeting_id = " + meetingId + " group by Meeting_id) as b on a.id = b.Meeting_id";
            DataTable dt = new DataTable("ExtraData");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(qry, con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [System.Web.Services.WebMethod()]
        public int getSubtaskId(int EID)
        { 
            int subtaskid = (@"select id from [spms].[dbo].[spms_tblSubTask] where eid = '" + EID + "' and action_code = 1 and subtask_description != '' order by id desc").Scalar(); 
            return subtaskid;
        }


        [System.Web.Services.WebMethod()]
        public DataTable get_other_subtask(int eid, string year, string semester,string ids)
        {

            string gg = ids.Replace("[","");
            string ids_new = gg.Replace("]","");

            int currentYear = DateTime.Now.Year;

            string add = @"<=6";
            if (semester == "2")
            {
                add = @">=7";
            }

            if (year == "" || year == null)
            {
                currentYear = DateTime.Now.Year;
            }
            else
            {
                currentYear = Convert.ToInt32(year);
            }


            DataTable dt = new DataTable("subtask");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select a.*,case when b.Remarks = '' or b.Remarks is null then 'none'  else b.Remarks end as remarks,
     CASE when c.ppa_id is null then 0 else 1 end as isppa, c.ppa_id,c.activity_id,c.accomplishment,c.DateTimeEntered,c.isOtherFunds,c.ppa_year  from ( select
	 id, 
     id as subtask_id,
	 task_id,
	 project_id,
	 subtask_description as description,
	 isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as start_date,
	 isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as end_date,  
	 CONVERT(VARCHAR(50),FORMAT(CAST(start_time as DateTime), 'hh:mm tt')) as start_time,
	 CONVERT(VARCHAR(50),FORMAT(CAST(end_time as DateTime), 'hh:mm tt')) as end_time, 
     eid,
	 CAST(is_done as int) as is_done  ,
	 CAST(is_verified as int ) as is_verified,
	 updated_on as updated,
	 CONVERT(VARCHAR(10), actual_start_date, 120) as actual_start_date,
	 CONVERT(VARCHAR(10), actual_end_date, 120) as actual_end_date,
	 CONVERT(VARCHAR(50),FORMAT(CAST(actual_start_time as DateTime), 'hh:mm tt')) as actual_start_time, 
	 CONVERT(VARCHAR(50),FORMAT(CAST(actual_end_time as DateTime), 'hh:mm tt')) as actual_end_time,  
	 CONVERT(varchar(20),start_date_time , 120 ) as start_date_time,
	 CONVERT(varchar(20),end_date_time , 120 ) as end_date_time,
	 isnull(target_accomplished, 0) target_accomplished,
     isalarm,
     alarm_option_id,
     output ,     
     action_code, 
     privacy, 
     IsTravel,
     Case when ControlNoID = '0' or ControlNoID = '' then '0' else ControlNoID  END as ControlNoID
     from [spms].[dbo].[spms_tblSubTask] where  eid = '" + eid + "' and id not in (" + ids_new + ") and action_code = 1 and subtask_description != '' and MONTH(actual_end_date) " + add.Replace("'", "") + " and YEAR(actual_start_date) = '" + currentYear + "' ) as a left join ( select * from ( select distinct SubtaskID,Remarks,ROW_NUMBER() Over (Partition By SubtaskID Order By id Desc) As Rn  from [spms].[dbo].[spms_tblAccomplishmentStatus_Logs]) as a where Rn = 1) as b on a.id = b.SubtaskID	 left join  [spms].[dbo].[spms_tblSubtask_PPA] as c on a.id = c.subtask_id order by actual_start_date  ", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [System.Web.Services.WebMethod()]
        public string AttachProof()
        {



            return "";
        }
       
#endregion

        [System.Web.Services.WebMethod()]
        public DataTable GetLeave(int eid, string year, string semester)
        { 
            int currentYear = 2019;

            if (year == "" || year == null)
            {
                currentYear = DateTime.Now.Year;
            }
            else
            {
                currentYear = Convert.ToInt32(year);
            }


            DataTable final = new DataTable("Leave");
            //final.Columns.Add("trid_emp", typeof(string));
            //final.Columns.Add("title", typeof(string));
            //final.Columns.Add("eid", typeof(int));
            //final.Columns.Add("min_date", typeof(string));
            //final.Columns.Add("max_date", typeof(string));


            DataTable dt1 = (@"Declare @UserID as int = "+eid+" Declare @isShowLeave as int = 1 "+
"select 0 as id,cast( DateOccur as date)   as 'start_date_time', " +
"cast( DateOccur as date)  as 'end_date_time', " +
"case when (select count(eid) from spms.dbo.splitstring(@UserID)) > 1 then b.EmpName + ' : ' else '' end + Reason collate SQL_Latin1_General_CP1_CI_AS as 'Description','Leave' as 'Type','#ce4c84' as Color "+
"from pmis.dbo.Not_swiping_reason as a "+
"left join pmis.dbo.m_vwgetallEmployee as b on b.eid = a.eid "+
"where case when @isShowLeave = 1 and a.eid in(select eid from spms.dbo.splitstring(@UserID)) then 'true' else 'false' end = 'true' and Reason in(select leaveDescription from pmis.dbo.m_tblLeaveType) and YEAR(a.DateOccur  ) = " + currentYear + "").DataSet(); 
            final.Merge(dt1);
        
            return final;
        }

        [System.Web.Services.WebMethod()]
        public DataTable GetPasco(int eid, string year, string semester)
        {
            int currentYear = 2019;

            if (year == "" || year == null)
            {
                currentYear = DateTime.Now.Year;
            }
            else
            {
                currentYear = Convert.ToInt32(year);
            }


            DataTable final = new DataTable("Leave");


            DataTable dt1 = (@"Declare @UserID as int = "+eid+"  "+
"Declare @isShowLeave as int = 1  "+
"select 0 as id,cast(inclusive_date as varchar(max))+ ' ' + cast(inclusive_time_from as varchar(max)) as start_date_time, "+
"cast(inclusive_date as varchar(max))+ ' ' + cast(inclusive_time_to as varchar(max)) as end_date_time, "+
"inclusive_date, "+
" CONVERT(VARCHAR(50),FORMAT(CAST(inclusive_time_from as DateTime), 'hh:mm tt')) as start_time  , "+
" CONVERT(VARCHAR(50),FORMAT(CAST(inclusive_time_to as DateTime), 'hh:mm tt')) as start_time  ,  "+
"case when (select count(eid) from spms.dbo.splitstring(@UserID)) > 1 then '[ <u>'+ b.EmpName + '</u> ] ' else '' end + 'PASCO : ' + r_reason  as Description, "+
"case when Isapproved = 0 then 'Submitted' else 'Leave' end as Type, "+
"case when Isapproved = 0 then '#8346c4' else '#ce4c84' end as Color "+
"from pmis.dbo.dilo_pasco_tks_tbl as a "+
"left join pmis.dbo.m_vwGetAllEmployee as b on b.eid = a.eid "+
"where a.eid in(select eid from spms.dbo.splitstring(@UserID)) and a.isactive = 1 and YEAR(a.inclusive_date) = " + currentYear + " ").DataSet();
            final.Merge(dt1); 
            return final;
        }

        [System.Web.Services.WebMethod()]
        public DataTable GetObas(int eid, string year, string semester)
        {
            int currentYear = 2019;

            if (year == "" || year == null)
            {
                currentYear = DateTime.Now.Year;
            }
            else
            {
                currentYear = Convert.ToInt32(year);
            }


            DataTable final = new DataTable("Leave");


            DataTable dt1 = (@"select  r_reason,r_date,r_date_to,time_from,time_to FROM [pmis].[dbo].[dilo_OBAS_tks_tbl] where eid = " + eid + " and YEAR(r_date) = "+currentYear+" and isactive = 1").DataSet();
            final.Merge(dt1);
            return final;
        }

        [System.Web.Services.WebMethod()]
        public DataTable GetOffice(int eid)
        {
            DataTable final = new DataTable("Office");
            try
            {
                using (SqlConnection con = new SqlConnection(common.pmis()))
                {
                    SqlCommand com = new SqlCommand(@"select Office from [pmis].[dbo].[employee] where eid = '" + eid + "'", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    final.Load(reader);
                    con.Close();
                }
                return final;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [System.Web.Services.WebMethod()]
        public DataTable GetOfficePPA(int officeid)
        {
            DataTable final = new DataTable("PPA");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select ppayear, isOtherFunds, ppaid, ppaName from [spms].[dbo].[vw_office_ppa] where implementingID = '" + officeid + "'", con);
                    //SqlCommand com = new SqlCommand(@"select ppaid, ppaYear, ppaName, isOtherFunds from [memis].[dbo].[vw_infraPPAs] where implementingID = '" + officeid + "'", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    final.Load(reader);
                    con.Close();
                }
                return final;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [System.Web.Services.WebMethod()]
        public DataTable GetPPAYear()
        {
            DataTable final = new DataTable("PPAYear");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(@"select distinct ppayear from [spms].[dbo].[vw_office_ppa]", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    final.Load(reader);
                    con.Close();
                }
                return final;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [System.Web.Services.WebMethod()]
        public DataTable GetActivity(int officeid)
        {
            DataTable final = new DataTable("Activity");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    //SqlCommand com = new SqlCommand(@"select isOtherFunds, activityid, activity from [spms].[dbo].[vw_ppa_activity] where ppaid = '" + ppaid + "'", con);
                    SqlCommand com = new SqlCommand(@";with ppaid as (SELECT ppaid from [spms].[dbo].[vw_office_ppa] where implementingID = '" + officeid + "') SELECT * from " +
                        "[spms].[dbo].[vw_ppa_activity] where ppaid IN (SELECT * from ppaid)", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    final.Load(reader);
                    con.Close();
                }
                return final;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [System.Web.Services.WebMethod()]
        public DataTable GetInfraPPA(int officeid)
        {
            DataTable final = new DataTable("InfraPPA");
            try
            {
                using (SqlConnection con = new SqlConnection(common.memis()))
                {
                    SqlCommand com = new SqlCommand(@"SELECT ppaid, ppaYear, implementingID, ppaName, ppaAmount, isnull(ppaLocation, '0') ppaLocation, durationDate FROM [memis].[dbo].[vw_infraPPAs] where implementingID = '"+officeid+"' ", con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    final.Load(reader);
                    con.Close();
                }
                return final;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void ReduceImageSizeAndSave(string filename, Image image)
        {
            foreach (var prop in image.PropertyItems)
            {
                if (prop.Id == 0x0112) //value of EXIF
                {
                    int orientationValue = image.GetPropertyItem(prop.Id).Value[0];
                    RotateFlipType rotateFlipType = GetOrientationToFlipType(orientationValue);
                    image.RotateFlip(rotateFlipType);
                    break;
                }
            }

            int baseScale = image.Width > image.Height ? image.Width : image.Height;
            double scaleFactor = Convert.ToDouble(320) / Convert.ToDouble(baseScale);

            // can given width of image as we want
            var newWidth = (int)(image.Width * scaleFactor);
            // can given height of image as we want
            var newHeight = (int)(image.Height * scaleFactor);

            var thumbnailImg = new Bitmap(newWidth, newHeight);




            var thumbGraph = Graphics.FromImage(thumbnailImg);
            thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
            thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
            thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
            thumbGraph.DrawImage(image, imageRectangle);

            Encoder myEncoder;
            myEncoder = Encoder.ColorDepth;

            EncoderParameter myEncoderParameter;
            myEncoderParameter =
            new EncoderParameter(myEncoder, 320);

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            myEncoderParameters.Param[0] = myEncoderParameter;

            ImageCodecInfo myImageCodecInfo;
            myImageCodecInfo = GetEncoderInfo("image/png");




            using (new ConnectToSharedFolder(networkPath, credentials))
            {
                try
                {
                    thumbnailImg.Save(filename, myImageCodecInfo, myEncoderParameters);

                }
                catch (Exception ex)
                {

                }
            }


        }

        public static RotateFlipType GetOrientationToFlipType(int orientationValue)
        {
            RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone;

            switch (orientationValue)
            {
                case 1:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
                case 2:
                    rotateFlipType = RotateFlipType.RotateNoneFlipX;
                    break;
                case 3:
                    rotateFlipType = RotateFlipType.Rotate180FlipNone;
                    break;
                case 4:
                    rotateFlipType = RotateFlipType.Rotate180FlipX;
                    break;
                case 5:
                    rotateFlipType = RotateFlipType.Rotate90FlipX;
                    break;
                case 6:
                    rotateFlipType = RotateFlipType.Rotate90FlipNone;
                    break;
                case 7:
                    rotateFlipType = RotateFlipType.Rotate270FlipX;
                    break;
                case 8:
                    rotateFlipType = RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
            }

            return rotateFlipType;
        }

        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

    }
}

/*
 * 
 * pencil sharper manual 212.55
 public DataTable jsonfeed(int stat,int eid)
        {
             
            string qry = "";

            if (stat == 1)
            {
                var officeid = (@"select Department FROM [pmis].[dbo].[vwMergeAllEmployee] where eid = '" + eid + "'").Scalar();
                qry = @"select TOP 50 a.id,b.EmpFullName as name,a.subtask_description as status,a.updated_on as timeStamp,'http://192.168.2.104/hris/Content/images/photos/'+cast(a.eid as nvarchar(20) ) +'.png' as profilePic  ,d.image from [spms].[dbo].[spms_tblSubTask]  as a inner join  [pmis].[dbo].[m_vwGetAllEmployee] as b on a.eid = b.eid inner join  (SELECT DISTINCT subtask_id, STUFF((SELECT ', ' + CONCAT('http://10.0.0.5/proof/',CAST(eid as nvarchar(20)),'/',attachment,'.',attachment_extension)  FROM spms_tblSubTaskProof as t2 WHERE t1.subtask_id = t2.subtask_id FOR XML PATH ('')),1,2, '') as image FROM spms_tblSubTaskProof as t1 GROUP BY t1.subtask_id ) as d on a.id = d.subtask_id  where b.Department = '" + officeid + "' order by id desc";
            }
            else 
            {
                qry = @"select TOP 50 a.id,b.EmpFullName as name,a.subtask_description as status,a.updated_on as timeStamp,
   'http://192.168.2.104/hris/Content/images/photos/'+cast(a.eid as nvarchar(20) ) +'.png' as profilePic  ,d.image
    from [spms].[dbo].[spms_tblSubTask]  as a inner join  [pmis].[dbo].[m_vwGetAllEmployee] as b on a.eid = b.eid inner join  (SELECT DISTINCT subtask_id, STUFF((SELECT ', ' + CONCAT('http://10.0.0.5/proof/',CAST(eid as nvarchar(20)),'/',attachment,'.',attachment_extension)  FROM spms_tblSubTaskProof as t2 WHERE t1.subtask_id = t2.subtask_id FOR XML PATH ('')),1,2, '') as image FROM spms_tblSubTaskProof as t1 GROUP BY t1.subtask_id ) as d on a.id = d.subtask_id order by id desc";
            }



            DataTable dt = new DataTable("feed");
            try
            {
                using (SqlConnection con = new SqlConnection(common.MyConnection()))
                {
                    SqlCommand com = new SqlCommand(qry, con);
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();
                    dt.Load(reader);
                    con.Close();
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
   [System.Web.Services.WebMethod()]
        public DataTable getForApproval(int eid )
        {
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            int sem = 1;
            string add = @"<=6";
            if (currentMonth > 6)
            {
                add = @">=7";
                sem = 2;
            }

            string qry = @"Declare @officeID int = this_officeid; 

with x as (select isnull((select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.raterID and
OfficeID = a.OfficeID and a.eid != a.raterID),
isnull((select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.RaterID and OfficeID = @OfficeID),
(select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.RaterID ))) as ParentSeriesID
from spms_tblOrganizationalChart as a
left join pmis.dbo.Employee as b on b.eid = a.eid
left join pmis.dbo.refsPositions as c on c.PositionCode = a.PositionID
left join pmis.dbo.OfficeDescription as d on d.OfficeID = a.OfficeID
left join spms_tblOffice as e on e.OfficeID = a.OfficeID
left join pmis.dbo.EDGE_tblPlantillaDivision as f on f.DivID = a.DivisionID
left join pmis.dbo.m_vwGetAllEmployee as g on g.eid = a.eid and g.isactive = 1
where a.OfficeID in(@officeID) 
)
select a.SeriesID,isnull((select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.raterID and
OfficeID = a.OfficeID and a.eid != a.raterID),
isnull((select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.RaterID and OfficeID = @OfficeID),
(select top 1 SeriesID from spms_tblOrganizationalChart where eid = a.RaterID ))) as ParentSeriesID,
b.Firstname + ' ' + left(mi,1) + '. ' + Lastname as EmployeeName, isnull(g.Position,c.Pos_name) as Position,
a.eid,d.OfficeAbbr,e.OfficeColor,f.DivName,a.RaterID,
a.PositionID,a.DivisionID,a.OfficeID,
case when a.eid in (select RaterID from spms_tblOrganizationalChart) then 1 else 0 end as isRater,
case when a.eid in (select RaterID from spms_tblOrganizationalChart) and a.OfficeID is not null 
then 'No. of Subordinates : ' + cast((select count(RaterID) from spms_tblOrganizationalChart where raterID = a.eid)
as varchar(10)) + '/' + cast((select count(RaterID) from spms_tblOrganizationalChart 
where OfficeID = a.OfficeID) as varchar(10)) else '' end as Subordinates
from spms_tblOrganizationalChart as a
left join pmis.dbo.Employee as b on b.eid = a.eid
left join pmis.dbo.refsPositions as c on c.PositionCode = a.PositionID
left join pmis.dbo.OfficeDescription as d on d.OfficeID = a.OfficeID
left join spms_tblOffice as e on e.OfficeID = a.OfficeID
left join pmis.dbo.EDGE_tblPlantillaDivision as f on f.DivID = a.DivisionID
left join pmis.dbo.m_vwGetAllEmployee as g on g.eid = a.eid and g.isactive = 1
where a.OfficeID in(@officeID) or a.SeriesID in(select x.ParentSeriesID from x) Order by a.DivisionID";



             int officeid = (@"select OfficeID from spms_tblOrganizationalChart where eid = "+eid).Scalar();



             DataTable Employees = (qry.Replace("this_officeid", officeid.ToString())).DataSet();

             DataRow[] select_row_data_rater = Employees.Select("RaterID = "+eid+"");

             string ids = "";
             
             foreach (DataRow row in select_row_data_rater)
             {
                 ids += ","+row["eid"];
             }



             DataTable todos = new DataTable("todos");

             if (ids == "")
             {
                 
             }
             else
             { 
                   try
                   {
                       using (SqlConnection con = new SqlConnection(common.MyConnection()))
                       {
                           SqlCommand com = new SqlCommand(@"select a.*,b.kpm_id,b.description as kpm_desc,b.quantity as kpm_target ,c.total from (select a.*,b.EmpFullName from 
        (select
	 id, 
     id as subtask_id,
	 task_id,
	 project_id,
	 subtask_description as description,
	 isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as start_date,
	 isnull(CONVERT(VARCHAR(10), start_date, 120),NULL) as end_date,  
	 CONVERT(VARCHAR(50),FORMAT(CAST(start_time as DateTime), 'hh:mm tt')) as start_time,
	 CONVERT(VARCHAR(50),FORMAT(CAST(end_time as DateTime), 'hh:mm tt')) as end_time, 
     eid,
	 CAST(is_done as int) as is_done  ,
	 CAST(is_verified as int ) as is_verified,
	 updated_on as updated,
	 CONVERT(VARCHAR(10), actual_start_date, 120) as actual_start_date,
	 CONVERT(VARCHAR(10), actual_end_date, 120) as actual_end_date,
	 CONVERT(VARCHAR(50),FORMAT(CAST(actual_start_time as DateTime), 'hh:mm tt')) as actual_start_time, 
	 CONVERT(VARCHAR(50),FORMAT(CAST(actual_end_time as DateTime), 'hh:mm tt')) as actual_end_time,  
	 CONVERT(varchar(20),start_date_time , 120 ) as start_date_time,
	 CONVERT(varchar(20),end_date_time , 120 ) as end_date_time,
	 target_accomplished, 
     output ,     
     action_code 
     from [spms].[dbo].[spms_tblSubTask]  where eid in (" + ids.Substring(1) + ") and is_verified = 0  and action_code = 1  and is_done = 1  and MONTH(actual_end_date) " + add.Replace("'", "") + " and YEAR(actual_start_date) = '" + currentYear + "' ) as a left join pmis.dbo.m_vwGetAllEmployee as b on a.eid = b.eid )as a inner join ( select a.id as individual_initiative_id, a.eid , b.id as outputid , c.id as kpm_id , Case When  c.baseline IS NULL THEN '0' ELSE c.baseline END as baseline , c.description,d.description as unit, e.*,f.id as project_id FROM [spms].[dbo].[spms_tblIndividualInitiatives] as a inner join [spms].[dbo].[spms_tblIndividualOutputs] as b on a.id = b.indi_initiative_id inner join [spms].[dbo].[spms_tblIndividualKPM] as c on b.id = c.Indivoutput_id inner join [spms].[dbo].[spms_tblUnitofMeasurements] as d on c.uom_id = d.id inner join [spms].[dbo].[spms_tblIndividualTargets] as e on c.id = e.indvkmp_id inner join  [spms].[dbo].[spms_tblTask] as f on e.id = f.TargetID where e.target_year = '" + currentYear + "' and semester_id = '" + sem + "') as b on a.project_id = b.project_id inner join (select sum(target_accomplished) as total,project_id from [spms].[dbo].[spms_tblSubTask]    group by project_id ) as c on b.project_id = c.project_id", con);
                           con.Open();
                           SqlDataReader reader = com.ExecuteReader();
                           todos.Load(reader);
                           con.Close();
                       }
                   }
                   catch (Exception ex)
                   {
                       throw ex;
                   }
             }
             return todos;
        }
*/ 