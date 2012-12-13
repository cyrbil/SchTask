using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchTask
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // vars
                string account = null, system = null, user = null, password = null, taskFilter = null, filters = null, returnFormat = "readable";
                string[] filtersArray = null;
                bool verbose = false;
                string[] validArgs = { "-a", "-s", "-u", "-p", "-t", "-f", "-r"};
                for (int i = 0; i < args.Length; i++)
                {
                    // c'est un argument, on regarde la suite.
                    if (validArgs.Contains(args[i]))
                    {
                        if (validArgs.Contains(args[i + 1])) // si c'est un arg, error
                            throw new Exception("L'argument " + args[i] + " attend comme paramètre une chaine de caractères");
                        // argument valide, on enregistre la string.
                        switch (args[i].ToLower())
                        {
                            case "-a": account = args[++i].Trim(); break;
                            case "-s": system = args[++i].Trim(); break;
                            case "-u": user = args[++i].Trim(); break;
                            case "-p": password = args[++i].Trim(); break;
                            case "-t": taskFilter = args[++i].Trim(); break;
                            case "-f": filters = args[++i].Trim(); break;
                            case "-r": returnFormat = args[++i].Trim(); break;
                        }
                    }
                    else if (args[i].Equals("-v"))// si c'est verbose, on active le mode
                        verbose = true;
                    else if (args[i].Equals("-h") || args[i].Equals("/?") || args[i].Equals("--help")) // si c'est help on display et quitte
                        return displayHelp();

                }
                if (!String.IsNullOrEmpty(filters)) // validation des filtres
                {
                    string[] validFilters = { 
                        "name", "trigger", "nextruntime", "enable", "lastrun",
                        "lastruntime", "lastrunreturn", "path",
                        "status", "command", "author", "description",
                        "executiontimelimit", "priority"
                    };
                    filtersArray = filters.Split(",".ToCharArray());
                    if(!filtersArray.All(str => validFilters.Contains(str.ToLower()))) {
                        throw new Exception("Bad filters. Valid filters are:\n\t\tname,trigger,nextruntime,enable,\n\t\tlastrun,lastruntime,lastrunreturn,path,\n\t\tstatus,command,author,description,\n\t\texecutiontimelimit,priority");
                    }
                }

                // arguments are ok and filtered, let's do it !
                new Scheduler(account, system, user, password, taskFilter, filtersArray, verbose, returnFormat);
            }
            catch (Exception e) {
                Console.WriteLine("An error occured: " + e.GetType() + "\n\t" + e.Message);
                Console.WriteLine("\nStack:\n" + e.StackTrace);
                return 2;
            }
            return 0;
        }

        private static int displayHelp()
        {
            Console.WriteLine("usage: ");
            Console.WriteLine("  -a    account           Spécifie l'account domaine pour la machine distante");
            Console.WriteLine("  -s    système           Spécifie le système distant auquel se connecter.");
            Console.WriteLine("  -u    utilisateur       Spécifie le contexte de l'utilisateur");
            Console.WriteLine("                          que schtasks.exe doit exécuter.");
            Console.WriteLine("  -p    [mot_passe]       Spécifie le mot de passe pour le contexte");
            Console.WriteLine("                          utilisateur donné. Il est demandé s'il est omis.");
            Console.WriteLine("  -t    task name         Filtre les resultat pour une tache");
            Console.WriteLine("  -v                      Affiche tout les détails sur le(s) tâche(s) demandée(s)");
            Console.WriteLine("  -f    filter[,f[...]]   Filtre les résultats en spécifiant la/les colonne(s) souhaitée(s)");
            Console.WriteLine("  -r    json/csv/readable Retourne les données dans le format choisi (readable = default)");
            Console.WriteLine("  -? / --help             Donne cette aide");
            return 1;
        }
    }
}
