using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.TaskScheduler;

namespace SchTask
{
    class Scheduler
    {
        private String account;
        private String system;
        private String user;
        private String password;
        private String taskFilter;
        private String[] filters = new String[] {};
        private String returnFormat;
        private Boolean verbose;
        private TaskService ts;
        private Task[] tasks;
        private List<Dictionary<String, String>> taskList = new List<Dictionary<String, String>>();
        

        public Scheduler(string account = null, string system = null, string user = null, string password = null, string taskFilter = null, string[] filters = null, bool verbose = false, string returnFormat = "readable")
        {
            this.account = account;
            this.system = system;
            this.user = user;
            this.password = password;
            this.taskFilter = (String.IsNullOrEmpty(taskFilter)) ? ".*" : "^" + taskFilter + "$";
            this.filters = filters;
            this.verbose = verbose;
            this.returnFormat = returnFormat;

            this.getSchedule();
        }

        private void getSchedule()
        {
            using (this.ts = new TaskService(system, user, account, password))
            {
                this.tasks = ts.FindAllTasks(new System.Text.RegularExpressions.Regex(taskFilter), true);
                foreach(Task task in this.tasks)
                    taskList.Add(filter(task));
                this.display();
            }
        }

        private Dictionary<String, String> filter(Task task)
        {
            /* Available filters: 
             *      name
             *      trigger (definition.triggers)
             *      nextRunTime
             *      enable
             *      lastRunTime
             *      lastRunReturn
             *      lastRun (lastRunTime + lastRunReturn)
             *      path
             *      status
             *      command (definition.action)
             *      author (definition.registrationinfo.author
             *      description (definition.registrationinfo.descrition)
             *      executionTimeLimit (definition.settings.executionTimeLimit)
             *      priority (definition.settings.priority)
             */
            Dictionary<String, String> r = new Dictionary<String, String>();


            if (this.filters == null)
            {// default filter:
                if (this.verbose)
                    this.filters = new string[] {
                        "name", "trigger", "nextruntime", "enable", "lastRun",
                        "path", "status", "command", "author",
                        "description", "executiontimelimit", "priority"
                    };
                else
                    this.filters = new string[] {
                        "name", "description", "trigger", "enable"
                    };
            }

            foreach (String filter in this.filters)
            {
                switch(filter.ToLower()) {
                    case "name":
                        r.Add(filter.ToLower(), task.Name);
                        break;
                    case "trigger":
                        r.Add(filter.ToLower(), task.Definition.Triggers.ToString());
                        break;
                    case "lastrun":
                        r.Add(filter.ToLower(), task.LastRunTime.ToString() + " (Return: " + task.LastTaskResult.ToString() + ")");
                        break;
                    case "enable":
                        r.Add(filter.ToLower(), (task.Enabled) ? "true" : "false");
                        break;
                    case "nextruntime":
                        r.Add(filter.ToLower(), task.LastRunTime.ToString());
                        break;
                    case "lastrunreturn":
                        r.Add(filter.ToLower(), task.LastTaskResult.ToString());
                        break;
                    case "path":
                        r.Add(filter.ToLower(), task.Path);
                        break;
                    case "status":
                        r.Add(filter.ToLower(), task.State.ToString());
                        break;
                    case "command":
                        r.Add(filter.ToLower(), task.Definition.Actions.ToString());
                        break;
                    case "author":
                        r.Add(filter.ToLower(), task.Definition.RegistrationInfo.Author);
                        break;
                    case "description":
                        r.Add(filter.ToLower(), task.Definition.RegistrationInfo.Description);
                        break;
                    case "executiontimelimit":
                        r.Add(filter.ToLower(), task.Definition.Settings.ExecutionTimeLimit.ToString());
                        break;
                    case "priority":
                        r.Add(filter.ToLower(), task.Definition.Settings.Priority.ToString());
                        break;
                }
            }
            return r;
        }

        private void display()
        {
            if (this.taskList.Count == 0)
            {
                throw new KeyNotFoundException("No Task Found for this filter.");
            }
            String[] separators = new String[] { "", "--------------------------\n", "", 
                                                    /* KEY */ " => ", /* VALUE */ "\n\n", 
                                                 "", "", null };
            
            switch (returnFormat.ToLower())
            {
                case "csv":
                    separators = new String[] { "", "", "\"", ",", "\n", "", "", "[\\\\\"]" };
                break;
                case "json":
                separators = new String[] { "[", "{", "\"", ":", ",", "}", "]", "[\\\\\"]" };
                break;
            }

            if(this.taskList.Count != 1) Console.Write(separators[0]);
            foreach(Dictionary<String, String> task in this.taskList)
            {
                Console.Write(separators[1]);
                foreach (KeyValuePair<String, String> property in task)
                {
                    Console.Write(separators[2] + property.Key + separators[2] + separators[3] + separators[2]);
                    if(!String.IsNullOrEmpty(separators[7]))
                        Console.Write(System.Text.RegularExpressions.Regex.Replace((property.Value == null) ? "" : property.Value, separators[7], "\\$0"));
                    else
                        Console.Write(property.Value);
                    Console.Write(separators[2]);
                    if (!property.Equals(task.Last())) Console.Write(separators[4]);
                }
                Console.Write(separators[5]);
                if (!task.Equals(this.taskList.Last())) Console.Write(separators[4]);
            }
            if (this.taskList.Count != 1) Console.Write(separators[6]);
        }
    }
}
