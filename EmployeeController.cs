using BAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using UI.Models;

namespace EAS.Controllers
{
    [EmployeesAuthenticate]
    public class EmployeeController : Controller
    {
        // GET: User

        Uri apiBaseAddress = new Uri("http://localhost:60428/api");
        HttpClient client;
        public EmployeeController()
        {
            client = new HttpClient();
            client.BaseAddress = apiBaseAddress;
        }
        public ActionResult Login()
        {
            return View();
        }

        #region Employee Login data post
        [HttpPost]
        public ActionResult EmployeeLogin(LoginModel objLogin)
        {
            bool status = false;
            string Message = "Failed";
            string PageRedirect = "";
            EmployeeModel employeeModel = new EmployeeModel();
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(objLogin);
                StringContent data = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage responseTask = client.PostAsync(client.BaseAddress + "/Employee/AutheticateEmployee", data).Result;
                if (responseTask.IsSuccessStatusCode)
                {
                    var readTask = responseTask.Content.ReadAsStringAsync().Result;
                    employeeModel = JsonConvert.DeserializeObject<EmployeeModel>(readTask);
                    status = true;
                    Message = "User has been authorised.";
                    PageRedirect = (employeeModel.Designation.ToLower() == "admin" ? "/Admin/Dashboard" : "/Employee/Dashboard");
                }
                else
                {
                    employeeModel = null;
                    status = false;
                    Message = "User has been unauthorise.";
                }
                Session["EmployeeDetails"] = employeeModel;
            }
            catch (AggregateException err)
            {
                status = false;
                Message = "Failed";
            }
            return Json(new { Status = status, Message = Message, PageRedirect = PageRedirect }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        public ActionResult Dashboard()
        {
            return View(Session["EmployeeDetails"] as EmployeeModel);
        }

        public ActionResult Profile()
        {
            return View(Session["EmployeeDetails"] as EmployeeModel);
        }
        public ActionResult LogOut()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        #region Attendance 

        public ActionResult GetEmployeeAndTimeDetails(string type)
        {
            if (type == "in")
                ViewBag.TimeStatus = "In";
            else
                ViewBag.TimeStatus = "Out";
            AttendanceModel attendanceModel = new AttendanceModel();
            attendanceModel.EmpId = Convert.ToInt32(Common.GetDataFromSession("EmpId"));
            attendanceModel.Mode = ViewBag.TimeStatus;
            attendanceModel.CheckInDate = DateTime.Now.ToString("yyyy-MM-dd");
            StringContent data = new StringContent(JsonConvert.SerializeObject(attendanceModel), System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage responseTask = client.PostAsync(client.BaseAddress + "/Employee/CheckAttendance", data).Result;
            if (responseTask.IsSuccessStatusCode)
                ViewBag.AttendanceMark = "Ok";

            return PartialView("_EmployeeCheckDetails", Session["EmployeeDetails"] as EmployeeModel);
        }

        public ActionResult SubmitAttendance(AttendanceModel attendanceModel)
        {
            bool status = false;
            string Message = "Failed";
            try
            {
                string DeviceType = "Window";
                string u = Request.ServerVariables["HTTP_USER_AGENT"];
                Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if ((b.IsMatch(u) || v.IsMatch(u.Substring(0, 4))))
                {
                    DeviceType = "Mobile";
                }
                attendanceModel.DeviceType = DeviceType;
                attendanceModel.MacAddress = GetMACAddress();
                attendanceModel.IPAddress = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
                attendanceModel.EmpId = Convert.ToInt32(Common.GetDataFromSession("EmpId"));
                if (attendanceModel.Mode.ToLower() == "in")
                {
                    attendanceModel.CheckInDate = DateTime.Now.ToString("yyyy-MM-dd");
                    attendanceModel.CheckInTime = DateTime.Now.ToString("hh:mm tt");
                }
                else
                {
                    attendanceModel.CheckOutDate = DateTime.Now.ToString("yyyy-MM-dd");
                    attendanceModel.CheckOutTime = DateTime.Now.ToString("hh:mm tt");
                }
                StringContent data = new StringContent(JsonConvert.SerializeObject(attendanceModel), System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage responseTask = client.PostAsync(client.BaseAddress + "/Employee/SaveAttendance", data).Result;
                if (responseTask.IsSuccessStatusCode)
                {
                    status = true;
                    Message = "Attendance has been marked.";
                }
                else
                {
                    status = false;
                    Message = (attendanceModel.Mode.ToLower() == "in" ? "Check in has not been marked" : "Check out has not been marked, before working hours.");
                }
            }
            catch (Exception ex)
            {
                status = false;
                Message = "Failed.";
            }
            return Json(new { Status = status, Message = Message });
        }

        public ActionResult TodayAttendance()
        {
            EmployeeAttendanceModel employee = new EmployeeAttendanceModel();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeTodayAttendance/" + Common.GetDataFromSession("EmpId")).Result;
            if (res.IsSuccessStatusCode)
            {
                employee = JsonConvert.DeserializeObject<EmployeeAttendanceModel>(res.Content.ReadAsStringAsync().Result);
            }
            return View(employee);
        }

        public string GetMACAddress1()
        {
            ManagementObjectSearcher objMOS = new ManagementObjectSearcher("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMOS.Get();
            string MACAddress = String.Empty;
            foreach (ManagementObject objMO in objMOC)
            {
                if (MACAddress == String.Empty) // only return MAC Address from first card   
                {
                    MACAddress = objMO["MacAddress"].ToString();
                }
                objMO.Dispose();
            }
            MACAddress = MACAddress.Replace(":", "");
            return MACAddress;
        }

        public string GetMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            return sMacAddress;
        }

        #endregion

        #region Change Password

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel changePasswordModel)
        {
            bool status = false;
            string Message = "Failed";
            changePasswordModel.EmpId = Convert.ToInt32(Common.GetDataFromSession("EmpId"));
            StringContent data = new StringContent(JsonConvert.SerializeObject(changePasswordModel), System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage responseTask = client.PostAsync(client.BaseAddress + "/Employee/ChangePassword", data).Result;
            if (responseTask.IsSuccessStatusCode)
            {
                status = true;
                Message = "Password has been changed.";
            }
            else
            {
                status = false;
                Message = "Password has not been changed.";
            }
            return Json(new { Status = status, Message = Message });
        }

        #endregion


        #region Leave Request

        public ActionResult LeaveRequest(string id)
        {
            char type = 'i';
            CommonSearch commonSearch = new CommonSearch();
            if (!string.IsNullOrEmpty(id) && TempData["DateRange"] != null)
            {
                string daterange = TempData["DateRange"] as string;
                string[] splitdaterange = daterange.Split('|');
                commonSearch.DateFrom = splitdaterange[0];
                commonSearch.DateTo = splitdaterange[1];
                type = 'u';
            }
            ViewBag.Type = type;
            ViewBag.RequestId = (!string.IsNullOrEmpty(id) ? id : "0");
            return View(commonSearch);
        }

        [HttpPost]
        public ActionResult EmployeeLeaveRequest(CommonSearch commonsearch)
        {
            DateTime Fromdate = Common.ConvertDateTimeMMDDYY(commonsearch.DateFrom);
            DateTime Todate = Common.ConvertDateTimeMMDDYY(commonsearch.DateTo);
            List<DateTime> dateTimes = new List<DateTime>();
            while (Fromdate <= Todate)
            {
                dateTimes.Add(Fromdate);
                DateTime d1 = Fromdate;
                Fromdate = d1.AddDays(1);
            }
            TempData["LeaveDuration"] = dateTimes;
            return Json(new { Status = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public PartialViewResult GetLeaveRequestData(CommonSearch commonsearch)
        {
            GetActiveLeaves();
            return PartialView("_EmployeeLeaveRequest");
        }
        private void GetActiveLeaves()
        {
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Admin/GetActiveLeaves").Result;
            if (res.IsSuccessStatusCode)
            {
                ViewBag.Leaves = JsonConvert.DeserializeObject<List<KeyValuePair<int, string>>>(res.Content.ReadAsStringAsync().Result);
            }
        }

        public PartialViewResult GetLeaveBalanceData()
        {
            List<LeaveAssign> lstleaveAssign = new List<LeaveAssign>();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeLeaveBalanceData/" + Common.GetDataFromSession("EmpId")).Result;
            if (res.IsSuccessStatusCode)
            {
                lstleaveAssign = JsonConvert.DeserializeObject<List<LeaveAssign>>(res.Content.ReadAsStringAsync().Result);
            }
            return PartialView("_EmployeeLeaveBalance", lstleaveAssign);
        }

        [HttpPost]
        public ActionResult SaveLeaveRequest(LeaveRequest leaveRequest)
        {
            bool status = false;
            string Message = "Failed";
            leaveRequest.EmpId = Convert.ToInt32(Common.GetDataFromSession("EmpId"));
            leaveRequest.Status = "Pending";
            leaveRequest.HolidayDates = (!string.IsNullOrEmpty(leaveRequest.HolidayDates) ? leaveRequest.HolidayDates : "");
            StringContent data = new StringContent(JsonConvert.SerializeObject(leaveRequest), System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage responseTask = client.PostAsync(client.BaseAddress + "/Employee/SaveLeaveRequest", data).Result;
            if (responseTask.IsSuccessStatusCode)
            {
                status = true;
                Message = "Leave Request has been successfully processed.";
            }
            else
            {
                status = false;
                Message = "Your Request has been not processed.";
            }
            return Json(new { Status = status, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetLeaveRequestById(int Rid)
        {
            LeaveRequest leaveRequest = new LeaveRequest();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeLeaveRequestById/" + Common.GetDataFromSession("EmpId") + "?Rid=" + Rid).Result;
            if (res.IsSuccessStatusCode)
            {
                leaveRequest = JsonConvert.DeserializeObject<LeaveRequest>(res.Content.ReadAsStringAsync().Result);
            }
            return Json(leaveRequest, JsonRequestBehavior.AllowGet);
        }

        


        #endregion

        #region Show Leave Request

        public ActionResult ShowLeaveRequest()
        {
            GetActiveLeaves();
            List<LeaveRequest> lstLeaveRequest = new List<LeaveRequest>();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeLeaveRequest/" + Common.GetDataFromSession("EmpId") + "?Type=Emp").Result;
            if (res.IsSuccessStatusCode)
            {
                lstLeaveRequest = JsonConvert.DeserializeObject<List<LeaveRequest>>(res.Content.ReadAsStringAsync().Result);
            }
            return View(lstLeaveRequest);
        }

        public ActionResult LeaveRequestLog(int Rid)
        {
            List<LeaveRequest> lstLeaveRequest = new List<LeaveRequest>();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeLeaveRequestLog/" + Common.GetDataFromSession("EmpId") + "?Rid=" + Rid).Result;
            if (res.IsSuccessStatusCode)
            {
                lstLeaveRequest = JsonConvert.DeserializeObject<List<LeaveRequest>>(res.Content.ReadAsStringAsync().Result);
            }
            return Json(lstLeaveRequest, JsonRequestBehavior.AllowGet);
        }


        public JavaScriptResult EditLeaveRequest(LeaveRequest leaveRequest)
        {
            TempData["DateRange"] = leaveRequest.DateRange;
            //return Redirect("/Employee/LeaveRequest/"+leaveRequest.Rid);
            return JavaScript("window.location = '/Employee/LeaveRequest/" + leaveRequest.Rid + "'");
        }

        #endregion

        #region EmployeeLeaveRequest

        public ActionResult EmployeeLeaveRequest()
        {
            GetActiveLeaves();
            List<LeaveRequest> lstLeaveRequest = new List<LeaveRequest>();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeLeaveRequest/" + Common.GetDataFromSession("EmpId") + "?Type=Manager").Result;
            if (res.IsSuccessStatusCode)
            {
                lstLeaveRequest = JsonConvert.DeserializeObject<List<LeaveRequest>>(res.Content.ReadAsStringAsync().Result);
            }
            return View(lstLeaveRequest);
        }

        public ActionResult GetEmployeeLeaveRequestLog(int EmpId, int Rid)
        {
            List<LeaveRequest> lstLeaveRequest = new List<LeaveRequest>();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeLeaveRequestLog/" + EmpId + "?Rid=" + Rid).Result;
            if (res.IsSuccessStatusCode)
            {
                lstLeaveRequest = JsonConvert.DeserializeObject<List<LeaveRequest>>(res.Content.ReadAsStringAsync().Result);
            }
            return Json(lstLeaveRequest, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult UpdateLeaveRequest(LeaveRequest leaveRequest)
        {
            bool status = false;
            string Message = "Failed";
            leaveRequest.LeaveReason = (!string.IsNullOrEmpty(leaveRequest.LeaveReason) ? leaveRequest.LeaveReason : "");
            StringContent data = new StringContent(JsonConvert.SerializeObject(leaveRequest), System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage responseTask = client.PutAsync(client.BaseAddress + "/Employee/UpdateLeaveRequest", data).Result;
            if (responseTask.IsSuccessStatusCode)
            {
                status = true;
                Message = "Leave Request has been successfully processed.";
            }
            else
            {
                status = false;
                Message = "This Request has been not processed.";
            }
            return Json(new { Status = status, Message = Message }, JsonRequestBehavior.AllowGet);

        }


        #endregion

        #region EmployeeLeaveRequest

        public ActionResult EmployeeAssetRequest()
        {
            GetActiveAssets();
            List<LeaveRequest> lstLeaveRequest = new List<LeaveRequest>();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeLeaveRequest/" + Common.GetDataFromSession("EmpId") + "?Type=Manager").Result;
            if (res.IsSuccessStatusCode)
            {
                lstLeaveRequest = JsonConvert.DeserializeObject<List<LeaveRequest>>(res.Content.ReadAsStringAsync().Result);
            }
            return View(lstLeaveRequest);
        }

        private void GetActiveAssets()
        {
            List<KeyValuePair<int, string>> lstActiveAsset = new List<KeyValuePair<int, string>>();
            lstActiveAsset.Add(new KeyValuePair<int, string>(0, "Select Asset"));
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Admin/GetActiveAsset").Result;
            if (res.IsSuccessStatusCode)
            {
                lstActiveAsset.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair<int, string>>>(res.Content.ReadAsStringAsync().Result));
                ViewBag.Assets = lstActiveAsset;
            }
        }

        public ActionResult GetAssetLeaveRequestLog(int EmpId, int Rid)
        {
            List<LeaveRequest> lstLeaveRequest = new List<LeaveRequest>();
            HttpResponseMessage res = client.GetAsync(client.BaseAddress + "/Employee/GetEmployeeLeaveRequestLog/" + EmpId + "?Rid=" + Rid).Result;
            if (res.IsSuccessStatusCode)
            {
                lstLeaveRequest = JsonConvert.DeserializeObject<List<LeaveRequest>>(res.Content.ReadAsStringAsync().Result);
            }
            return Json(lstLeaveRequest, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EmployeeAssetRequest(CommonSearch commonsearch)
        {
           
            return Json(new { Status = true }, JsonRequestBehavior.AllowGet);
        }


        #endregion

    }
}