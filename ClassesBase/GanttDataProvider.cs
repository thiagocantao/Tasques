using DevExpress.CodeParser;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Office.Utils;
using DevExpress.Web.ASPxGantt;
using DevExpress.XtraCharts;
using DevExpress.XtraRichEdit.Fields;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace BriskPtf.ClassesBase
{

    public static class GanttDataProvider
    {
        
        const string
            TasksSessionKey = "Tasks",
            DependenciesSessionKey = "Dependencies",
            ResourcesSessionKey = "Resources",
            ResourceAssignmentsSessionKey = "ResourceAssignments";

        public static string codigoProjeto;
        public static string codigoRecurso;
        public static string versaoLB;
        public static bool atualizaDados;
        public static bool somenteAtrasadas;
        public static bool somenteMarcos;
        public static bool removeIdentacao = false;

        static HttpSessionState Session { get { return HttpContext.Current.Session; } }

        public static object GetTasks(string idProjeto, string codRec, string linhaDeBase, string AtualizaDados, string SomenteAtrasadas, string SomenteMarcos) {
            codigoProjeto = idProjeto;
            codigoRecurso = codRec;
            versaoLB = linhaDeBase;
            atualizaDados = AtualizaDados == "S";
            somenteAtrasadas = SomenteAtrasadas == "S";
            somenteMarcos = SomenteMarcos == "S";
            return Tasks; 
        }
        public static object GetDependencies() { return Dependencies; }
        public static object GetResources() { return Resources; }
        public static object GetResourceAssignments() { return ResourceAssignments; }

        public static List<Task> Tasks
        {
            
            get
            {
                if (Session[TasksSessionKey] == null)
                    Session[TasksSessionKey] = CreateTasks();
                return ((List<Task>)Session[TasksSessionKey]).FindAll(t => !t.tipoEdicao.Equals("D") && !t.tipoEdicao.Equals("E"));
            }
        }
        public static List<Dependency> Dependencies
        {
            get
            {
                if (Session[DependenciesSessionKey] == null)
                    Session[DependenciesSessionKey] = CreateDependencies();
                return (List<Dependency>)Session[DependenciesSessionKey];
            }
        }
        public static List<Resource> Resources
        {
            get
            {
                if (Session[ResourcesSessionKey] == null)
                    Session[ResourcesSessionKey] = CreateResources();
                return (List<Resource>)Session[ResourcesSessionKey];
            }
        }
        public static List<ResourceAssignment> ResourceAssignments
        {
            get
            {
                if (Session[ResourceAssignmentsSessionKey] == null)
                    Session[ResourceAssignmentsSessionKey] = CreateResourceAssignments();
                return (List<ResourceAssignment>)Session[ResourceAssignmentsSessionKey];
            }
        }

        static List<Task> CreateTasks()
        {
            var result = new List<Task>();
            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();
            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            dados cDados = CdadosUtil.GetCdados(listaParametrosDados);



            if (codigoRecurso != "-1" || somenteMarcos || somenteAtrasadas)
            {
                removeIdentacao = true;
            }
            else
                removeIdentacao = false;

            DataSet dsCrono = cDados.getCronogramaGantt(int.Parse(codigoProjeto), codigoRecurso, int.Parse(versaoLB),removeIdentacao, somenteAtrasadas, somenteMarcos, null, null);
            

            foreach (DataRow dr in dsCrono.Tables[0].Rows)
            {
                string idTarefa = dr["CodigoRealTarefa"].ToString().Trim();
                string tarefaSuperior = removeIdentacao ? "" : dr["TarefaSuperior"].ToString();

                Color corTarefa = Color.LightBlue;

                if (dr["TerminoReal"].ToString() != "")
                    corTarefa = Color.Green;
                else if (dr["IndicaCritica"].ToString() != "1")
                    corTarefa = Color.Purple;

                result.Add(CreateTask(idTarefa, tarefaSuperior, dr["NomeTarefa"].ToString(), DateTime.Parse(dr["Inicio"].ToString())
                    , DateTime.Parse(dr["Termino"].ToString()), (int)Double.Parse(dr["Concluido"].ToString()), "", (dr["InicioLB"] + "" == "") ? DateTime.MinValue : (DateTime.Parse(dr["InicioLB"].ToString())),
                    (dr["TerminoLB"] + "" == "") ? DateTime.MinValue : (DateTime.Parse(dr["TerminoLB"].ToString())), Double.Parse(dr["PercentualPrevisto"].ToString()), Double.Parse(dr["ValorPesoTarefaLB"].ToString()),
                    Double.Parse(dr["PercentualPesoTarefa"].ToString()), Double.Parse(dr["Duracao"].ToString()), Double.Parse(dr["Trabalho"].ToString()),
                    (dr["TerminoReal"] + "" == "") ? DateTime.MinValue : (DateTime.Parse(dr["TerminoReal"].ToString())), dr["CodigoTarefa"].ToString(), corTarefa));
            }            

            return result;
        }
        public static Task CreateTask(string id, string parentid, string subject,
            DateTime start, DateTime end, int percent, string resources, DateTime startBaseLine, DateTime endBaseLine,
            double previsto, double pesoTarefaLB, double percentualPesoTarefa, double duracao, double trabalho, DateTime terminoReal, string codigoTarefa, Color corTarefa)
        {
            var task = new Task();
            task.ID = id;
            task.ParentID = parentid;
            task.Tarefa = subject;
            task.Inicio = start;
            task.Termino = end;
            task.PercentualRealizado = percent;
            task.Employees = resources;
            task.InicioLB = startBaseLine;
            task.TerminoLB = endBaseLine;
            task.PercentualPrevisto = previsto;
            task.ValorPesoTarefa = pesoTarefaLB;
            task.PercentualPesoTarefa = percentualPesoTarefa;
            task.Duracao = duracao;
            task.Trabalho = trabalho;
            task.TerminoReal = terminoReal;
            task.CodigoTarefa = codigoTarefa;
            task.CorTarefa = corTarefa;
            task.tipoEdicao = "";

            return task;
        }
        static List<Dependency> CreateDependencies()
        {
            var result = new List<Dependency>();
            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();
            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            if (removeIdentacao)
                return result;

            dados cDados = CdadosUtil.GetCdados(listaParametrosDados);
            DataSet ds = cDados.getDataSet(string.Format(@"
                SELECT tcpp.codigoTarefa AS TarefaTo, tcpp.codigoTarefaPredecessora AS TarefaFrom, tipoLatencia
                  FROM {0}.{1}.[TarefaCronogramaProjetoPredecessoras] tcpp INNER JOIN
			           {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = tcpp.CodigoCronogramaProjeto 
													AND cp.CodigoProjeto = {2})", cDados.getDbName(), cDados.getDbOwner(), codigoProjeto));


            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string tipoConector = dr["tipoLatencia"].ToString();
                if (tipoConector == "" || tipoConector == "TI")
                    tipoConector = "2";
                else if (tipoConector == "II")
                    tipoConector = "0";
                else if (tipoConector == "TT")
                    tipoConector = "3";
                else if (tipoConector == "IT")
                    tipoConector = "1";



                result.Add(new Dependency() { ID = CreateUniqueId(), Type = int.Parse(tipoConector), ParentID = GetTaskByCode(dr["TarefaFrom"].ToString()), DependentID = GetTaskByCode(dr["TarefaTo"].ToString()) });
            }
            return result;
        }
        static List<Resource> CreateResources()
        {
            var result = new List<Resource>();
            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();
            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            if (removeIdentacao)
                return result;

            dados cDados = CdadosUtil.GetCdados(listaParametrosDados);

            DataSet dsRecursos = cDados.getRecursosCronograma(int.Parse(codigoProjeto), "");

            foreach(DataRow dr in dsRecursos.Tables[0].Rows)
            {
                result.Add(new Resource() { ID = dr["CodigoRecurso"].ToString(), Name = dr["Recurso"].ToString() });
            }
            
            return result;
        }
        static List<ResourceAssignment> CreateResourceAssignments()
        {
            var result = new List<ResourceAssignment>();

            if (removeIdentacao)
                return result;

            foreach (Task task in Tasks)
            {
                if (!string.IsNullOrEmpty(task.Employees))
                {
                    string[] empIDs = task.Employees.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < empIDs.Length; i++)
                        result.Add(new ResourceAssignment() { ID = CreateUniqueId(), TaskID = task.ID, ResourceID = empIDs[i] });
                }
            }
            return result;
        }

        public static void UpdateTask(Task task)
        {
            if (task.ID == null)
                return;

            string tipoAlteracao = "";

            Task item = Tasks.FirstOrDefault(c => c.ID.Equals(task.ID));

            if (item.Inicio != task.Inicio || item.Termino != task.Termino)
                tipoAlteracao = "DATAS";
            else if (item.Duracao != task.Duracao)
                tipoAlteracao = "DURACAO";
            else if (item.Trabalho != task.Trabalho)
                tipoAlteracao = "TRABALHO";

            item.ParentID = task.ParentID;
            item.PercentualRealizado = task.PercentualRealizado;
            item.Inicio = task.Inicio;
            item.Termino = task.Termino;
            item.Tarefa = task.Tarefa;
            item.tipoEdicao = "U";
            item.InicioLB = task.InicioLB == DateTime.MinValue ? null : task.InicioLB;
            item.TerminoLB = task.TerminoLB == DateTime.MinValue ? null : task.TerminoLB;
            item.Duracao = task.Duracao;
            item.Trabalho = task.Trabalho;
            item.TerminoReal = task.TerminoReal == DateTime.MinValue ? null : task.TerminoReal;

            AtualizarTarefa(Tasks, task.ID, task.Duracao, task.Trabalho, task.Inicio, task.Termino, tipoAlteracao);
        }

        static void AtualizarTarefa(List<Task> tasks, string tarefaModificadaID, double novaDuracao, double novoTrabalho, DateTime novoInicio, DateTime novoTermino, string tipoAlteracao)
        {
            // Criar um dicionário para acesso rápido às tarefas pelo ID
            Dictionary<string, Task> taskDict = tasks.ToDictionary(t => t.ID);

            // Obtém a tarefa modificada
            Task tarefaModificada = taskDict[tarefaModificadaID];

            // Verifica o tipo de alteração
            if (tipoAlteracao == "DURACAO")
            {
                // Atualiza duração e recalcula término
                tarefaModificada.Duracao = novaDuracao;
                tarefaModificada.Termino = tarefaModificada.Inicio.AddDays(novaDuracao);
            }
            else if (tipoAlteracao == "DATAS")
            {
                // Atualiza datas e recalcula duração
                tarefaModificada.Inicio = novoInicio;
                tarefaModificada.Termino = novoTermino;
                tarefaModificada.Duracao = (novoTermino - novoInicio).TotalDays;
            }

            // Atualiza o trabalho
            tarefaModificada.Trabalho = novoTrabalho;

            // Propaga a atualização para os pais
            string parentID = tarefaModificada.ParentID;
            while (!string.IsNullOrEmpty(parentID) && taskDict.ContainsKey(parentID))
            {
                // Obtém o pai
                Task tarefaPai = taskDict[parentID];

                // Atualiza duração e trabalho somando os valores dos filhos
                tarefaPai.Duracao = tasks.Where(t => t.ParentID == parentID).Sum(t => t.Duracao);
                tarefaPai.Trabalho = tasks.Where(t => t.ParentID == parentID).Sum(t => t.Trabalho);

                // Atualiza datas do pai com base nas datas mínimas e máximas dos filhos
                var filhos = tasks.Where(t => t.ParentID == parentID).ToList();
                tarefaPai.Inicio = filhos.Min(t => t.Inicio);
                tarefaPai.Termino = filhos.Max(t => t.Termino);

                tarefaPai.tipoEdicao = "U";

                // Passa para o próximo nível na hierarquia
                parentID = tarefaPai.ParentID;
            }
        }

        static void ExcluirTarefa(List<Task> tasks, string parentID)
        {
            // Criar um dicionário para acesso rápido às tarefas pelo ID
            Dictionary<string, Task> taskDict = tasks.ToDictionary(t => t.ID);


            // Propaga a atualização para os pais
            while (!string.IsNullOrEmpty(parentID) && taskDict.ContainsKey(parentID))
            {
                // Obtém o pai
                Task tarefaPai = taskDict[parentID];

                // Filtra os filhos restantes
                var filhos = tasks.Where(t => t.ParentID == parentID).ToList();

                if (filhos.Any())
                {
                    // Se ainda há filhos, recalcula os valores do pai
                    tarefaPai.Duracao = filhos.Sum(t => t.Duracao);
                    tarefaPai.Trabalho = filhos.Sum(t => t.Trabalho);
                    tarefaPai.Inicio = filhos.Min(t => t.Inicio);
                    tarefaPai.Termino = filhos.Max(t => t.Termino);
                }

                // Passa para o próximo nível na hierarquia
                parentID = tarefaPai.ParentID;
            }
        }

        public static string InsertTask(Task task)
        {
            task.ID = CreateUniqueId();
            task.tipoEdicao = "I";
            Tasks.Add(task);
            return task.ID;
        }
        public static void DeleteTask(Task task)
        {
            Task taskToDelete = Tasks.FirstOrDefault(t => t.ID.Equals(task.ID));
            if (taskToDelete != null)
                taskToDelete.tipoEdicao = "D";

            ExcluirTarefa(Tasks, taskToDelete.ParentID);
        }

        public static string GetTaskByCode(string codigoTarefa)
        {
            return Tasks.FirstOrDefault(t => t.CodigoTarefa.Equals(codigoTarefa)).ID;
        }

        public static void DeleteTaskByKey(string key)
        {
            Task taskToDelete = Tasks.FirstOrDefault(t => t.ID.Equals(key));
            if (taskToDelete != null)
                taskToDelete.tipoEdicao = "D";
            ExcluirTarefa(Tasks, taskToDelete.ParentID);
        }

        public static string InsertDependency(Dependency dependency)
        {
            dependency.ID = CreateUniqueId();
            Dependencies.Add(dependency);
            return dependency.ID;
        }
        public static void DeleteDependency(Dependency dependency)
        {
            var dependencyToDelete = Dependencies.FirstOrDefault(t => t.ID.Equals(dependency.ID));
            if (dependencyToDelete != null)
                Dependencies.Remove(dependencyToDelete);
        }
        public static void DeleteDependencyByKey(string key)
        {
            var dependencyToDelete = Dependencies.FirstOrDefault(t => t.ID.Equals(key));
            if (dependencyToDelete != null)
                Dependencies.Remove(dependencyToDelete);
        }

        public static void UpdateResource(Resource resource)
        {
            Resource item = Resources.FirstOrDefault(c => c.ID.Equals(resource.ID));
            item.Name = resource.Name;
        }

        public static string InsertResource(Resource resource)
        {
            resource.ID = CreateUniqueId();
            Resources.Add(resource);
            return resource.ID;
        }
        public static void DeleteResource(Resource resource)
        {
            var resourceToDelete = Resources.FirstOrDefault(t => t.ID.Equals(resource.ID));
            if (resourceToDelete != null)
                Resources.Remove(resourceToDelete);
        }

        public static void DeleteResourceByKey(string key)
        {
            var resourceToDelete = Resources.FirstOrDefault(t => t.ID.Equals(key));
            if (resourceToDelete != null)
                Resources.Remove(resourceToDelete);
        }

        public static string InsertResourceAssignment(ResourceAssignment resourceAssignment)
        {
            resourceAssignment.ID = CreateUniqueId();
            ResourceAssignments.Add(resourceAssignment);
            return resourceAssignment.ID;
        }
        public static void DeleteResourceAssignment(ResourceAssignment resourceAssignment)
        {
            var itemToDelete = ResourceAssignments.FirstOrDefault(t => t.ID.Equals(resourceAssignment.ID));
            if (itemToDelete != null)
                ResourceAssignments.Remove(itemToDelete);
        }

        public static void DeleteResourceAssignmentByKey(string key)
        {
            var itemToDelete = ResourceAssignments.FirstOrDefault(t => t.ID.Equals(key));
            if (itemToDelete != null)
                ResourceAssignments.Remove(itemToDelete);
        }

        static string CreateUniqueId() { return Guid.NewGuid().ToString(); }
    }


    public class Task
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Tarefa { get; set; }
        public string Description { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Termino { get; set; }
        public int PercentualRealizado { get; set; }
        public DateTime? InicioLB { get; set; }
        public DateTime? TerminoLB { get; set; }
        public string Employees { get; set; }
        public double PercentualPrevisto { get; set; }
        public double ValorPesoTarefa { get; set; }
        public double PercentualPesoTarefa { get; set; }
        public double Duracao { get; set; }
        public double Trabalho { get; set; }
        public DateTime? TerminoReal { get; set; }
        public string CodigoTarefa { get; set; }
        public Color CorTarefa { get; set; }
        public string tipoEdicao { get; set; }

    }         
               
                
                
    

    public class Projeto
    {
        private int codigoProjeto; // private field

        public int CodigoProjeto// public property
        {
            get { return codigoProjeto; } // getter
            set { codigoProjeto = value; } // setter
        }
    }

    public class Dependency
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string DependentID { get; set; }
        public int Type { get; set; }
    }
    public class Resource
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
    public class ResourceAssignment
    {
        public string ID { get; set; }
        public string TaskID { get; set; }
        public string ResourceID { get; set; }

    }
}
