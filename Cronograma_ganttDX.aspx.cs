using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DevExpress.Web;
using System.Web.Hosting;
using System.IO;
using System.Xml;
using CDIS;
using BriskPtf.GanttHelper;
using BriskPtf.ClassesBase;
using DevExpress.XtraRichEdit.Layout.Engine;
using DevExpress.XtraCharts;
using System.Collections.Generic;
using System.Linq;
using DevExpress.XtraRichEdit.Model;

namespace BriskPtf._Projetos.DadosProjeto
{
    public partial class _Projetos_DadosProjeto_Cronograma_ganttDX : System.Web.UI.Page
    {
        dados cDados;

        public int codigoProjeto = 0;

        public string alturaGrafico = "", larguraGrafico = "", nenhumGrafico = "";
        public string nomeProjeto = "";
        public int codigoEntidadeUsuarioResponsavel = -1;
        private string utilizaCronoInstalado = "N", utilizaNovaEAP = "N";
        int codigoUsuario = -1;

        private string imprimeDadosLinhaBaseCronograma = "N";
        private int versaoLinhaBase;
        string codigoCronogramaReplanejamento = "", codigoCronogramaOficial = "", eapBloqueadaEm = "", eapBloqueadaPor = "", codigoCronogramaProjetoBloqueado = "";
        bool edicaoEAP = false;
        private CdisGanttHelper cdisGanttHelper;


        protected void Page_Init(object sender, EventArgs e)
        {
            DevExpress.Web.ASPxWebControl.SetIECompatibilityModeEdge();
            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            cDados = CdadosUtil.GetCdados(listaParametrosDados);
            try
            {
                if (cDados.getInfoSistema("IDUsuarioLogado") == null)
                    Response.Redirect("~/erros/erroInatividade.aspx");
            }
            catch
            {
                Response.RedirectLocation = cDados.getPathSistema() + "erros/erroInatividade.aspx";
                Response.End();
            }

            codigoEntidadeUsuarioResponsavel = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());
            codigoUsuario = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());




            cDados.aplicaEstiloVisual(this);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            // a pagina não pode ser armazenada no cache
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            try
            {
                if (cDados.getInfoSistema("IDUsuarioLogado") == null)
                    Response.Redirect("~/erros/erroInatividade.aspx");
            }
            catch
            {
                Response.RedirectLocation = cDados.getPathSistema() + "erros/erroInatividade.aspx";
                Response.End();
            }

            string cssPostfix = "", cssPath = "";

            cDados.getVisual(cDados.getInfoSistema("IDEstiloVisual").ToString(), ref cssPath, ref cssPostfix);




            if (Request.QueryString["IDProjeto"] != null && Request.QueryString["IDProjeto"].ToString() != "")
            {
                codigoProjeto = int.Parse(Request.QueryString["IDProjeto"].ToString());
            }

            if (Request.QueryString["NP"] != null && Request.QueryString["NP"].ToString() != "")
            {
                nomeProjeto = Request.QueryString["NP"].ToString();
            }




            if (!IsPostBack)
            {
                int codigoUsuario = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
                int codigoEntidade = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

                cDados.VerificaAcessoTelaSemMaster(this, codigoUsuario, codigoEntidade, codigoProjeto, "null", "PR", 0, "null", "PR_CnsCrn");
            }

            DataSet dsParam = cDados.getParametrosSistema(codigoEntidadeUsuarioResponsavel, "utilizaCronoInstalado", "utilizaNovaEAP");

            if (cDados.DataSetOk(dsParam) && cDados.DataTableOk(dsParam.Tables[0]))
            {
                if (dsParam.Tables[0].Rows[0]["utilizaCronoInstalado"].ToString() != "")
                    utilizaCronoInstalado = dsParam.Tables[0].Rows[0]["utilizaCronoInstalado"].ToString();

                if (dsParam.Tables[0].Rows[0]["utilizaNovaEAP"].ToString() != "")
                    utilizaNovaEAP = dsParam.Tables[0].Rows[0]["utilizaNovaEAP"].ToString();
            }

            verificaPermissoesBotoes();

            carregaComboRecursos();
            carregaComboLinhaBase();
            defineOpcoesEAP();

            TasksDataSource.SelectParameters.Clear();
            TasksDataSource.SelectParameters.Add("idProjeto", codigoProjeto.ToString());
            TasksDataSource.SelectParameters.Add("codRec", ddlRecurso.Value.ToString());
            TasksDataSource.SelectParameters.Add("linhaDeBase", ddlLinhaBase.Value.ToString());
            TasksDataSource.SelectParameters.Add("AtualizaDados", "S");
            TasksDataSource.SelectParameters.Add("SomenteAtrasadas", "N");
            TasksDataSource.SelectParameters.Add("SomenteMarcos", "N");
            

            defineLarguraTela();
        }

        private void defineLarguraTela()
        {
            string ResolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString().ToString();
            int largura = int.Parse(ResolucaoCliente.Substring(0, ResolucaoCliente.IndexOf('x')));
            int altura = int.Parse(ResolucaoCliente.Substring(ResolucaoCliente.IndexOf('x') + 1));

            alturaGrafico = (altura - 240).ToString();
            larguraGrafico = (largura - 175).ToString();

            Gantt.Height = altura - 300;
        }

        private void carregaCronograma()
        {
            

        }

        private void carregaComboRecursos()
        {
            DataSet dsRecursos = cDados.getRecursosCronograma(codigoProjeto, "");

            if (cDados.DataSetOk(dsRecursos))
            {
                ddlRecurso.DataSource = dsRecursos;

                ddlRecurso.TextField = "Recurso";

                ddlRecurso.ValueField = "CodigoRecurso";

                ddlRecurso.DataBind();
            }

            ListEditItem lei = new ListEditItem(Resources.traducao.todos, "-1");

            ddlRecurso.Items.Insert(0, lei);

            if (!IsPostBack)
                ddlRecurso.SelectedIndex = 0;
        }

        protected void ASPxButton5_Click(object sender, EventArgs e)
        {
            Session["Tasks"] = null;
            Session["Dependencies"] = null;
            Session["Resources"] = null;
            Session["ResourceAssignments"] = null;

            TasksDataSource.SelectParameters.Clear();
            TasksDataSource.SelectParameters.Add("idProjeto", codigoProjeto.ToString());
            TasksDataSource.SelectParameters.Add("codRec", ddlRecurso.Value.ToString());
            TasksDataSource.SelectParameters.Add("linhaDeBase", ddlLinhaBase.Value.ToString());
            TasksDataSource.SelectParameters.Add("AtualizaDados", "S");
            TasksDataSource.SelectParameters.Add("SomenteAtrasadas", ckTarefasAtrasadas.Checked ? "S" : "N");
            TasksDataSource.SelectParameters.Add("SomenteMarcos", ckMarcos.Checked ? "S" : "N");
            //TasksDataSource.DataBind();
            //TasksDataSource.Select();
            //Gantt.DataBind();

        }

        //static void AtualizarTarefa(ObjectDataSource dataSource, string tarefaModificadaID, double novaDuracao, double novoTrabalho)
        //{
        //    // Obtém a lista de tarefas do ObjectDataSource
        //    List<Task> tasks = (List<Task>)dataSource.Select();

        //    // Criar um dicionário para acesso rápido às tarefas pelo ID
        //    Dictionary<string, Task> taskDict = tasks.ToDictionary(t => t.ID);

            

        //    // Atualiza a duração e trabalho da tarefa modificada
        //    taskDict[tarefaModificadaID].Duracao = novaDuracao;
        //    taskDict[tarefaModificadaID].Trabalho = novoTrabalho;
        //    taskDict[tarefaModificadaID].tipoEdicao = "E";

        //    // Propaga a atualização para os pais
        //    string parentID = taskDict[tarefaModificadaID].ParentID;
        //    while (!string.IsNullOrEmpty(parentID) && taskDict.ContainsKey(parentID))
        //    {
        //        // Soma as durações e trabalhos de todas as tarefas filhas do pai atual
        //        double novaDuracaoPai = tasks.Where(t => t.ParentID == parentID).Sum(t => t.Duracao);
        //        double novoTrabalhoPai = tasks.Where(t => t.ParentID == parentID).Sum(t => t.Trabalho);

        //        // Atualiza o pai
        //        taskDict[parentID].Duracao = novaDuracaoPai;
        //        taskDict[parentID].Trabalho = novoTrabalhoPai;
        //        taskDict[parentID].tipoEdicao = "E";

        //        // Passa para o próximo nível na hierarquia
        //        parentID = taskDict[parentID].ParentID;
        //    }

        //    // Atualiza o ObjectDataSource com a nova lista modificada
        //    dataSource.Update();
        //}

        protected void TasksDataSource_Updated(object sender, ObjectDataSourceStatusEventArgs e)
        {
            //// Pegando os dados da tarefa que foi atualizada
            //if (e.ReturnValue is Task updatedTask)
            //{
              
            //    // Chamando a função para atualizar os pais
            //    AtualizarTarefa((ObjectDataSource)sender, updatedTask.ID, updatedTask.Duracao, updatedTask.Trabalho);
            //}
        }

        protected void Gantt_TaskUpdated(object sender, DevExpress.Web.Data.ASPxDataUpdatedEventArgs e)
        {
            //if(e.NewValues != null)
            //{
            //    AtualizarTarefa(TasksDataSource, e.Keys[0] + "",double.Parse(e.NewValues["Duracao"] + ""), double.Parse(e.NewValues["Trabalho"] + ""));
            //}
        }
        private void carregaComboLinhaBase()
        {
            DataSet dsLinhaBase = cDados.getVersoesLinhaBase(codigoProjeto, "");

            if (cDados.DataSetOk(dsLinhaBase))
            {
                ddlLinhaBase.DataSource = dsLinhaBase;

                ddlLinhaBase.TextField = "VersaoLinhaBase";

                ddlLinhaBase.ValueField = "NumeroVersao";

                ddlLinhaBase.DataBind();
            }

            if (!IsPostBack)
                ddlLinhaBase.SelectedIndex = 0;

            carregaInformacoesLBSelecionada();
        }

        private void carregaInformacoesLBSelecionada()
        {
            if (ddlLinhaBase.SelectedIndex != -1)
            {
                DataSet dsLinhaBase = cDados.getDetalhesLinhaBase(codigoProjeto, "" + ddlLinhaBase.Value);

                if (cDados.DataSetOk(dsLinhaBase) && cDados.DataTableOk(dsLinhaBase.Tables[0]))
                {
                    DataRow dr = dsLinhaBase.Tables[0].Rows[0];

                    txtVersao.Text = dr["Descricao"].ToString();
                    txtStatus.Text = dr["StatusLB"].ToString();
                    txtDataSolicitacao.Value = dr["DataSolicitacao"];
                    txtSolicitante.Text = dr["Solicitante"].ToString();
                    txtDataAprovacao.Value = dr["DataAprovacao"];
                    txtAprovador.Text = dr["Aprovador"].ToString();
                    txtAnotacao.Text = dr["Anotacao"].ToString();
                }
            }
        }

        protected void btnEditarCronograma_Click(object sender, EventArgs e)
        {
            //abreCronograma();
        }
        private void defineOpcoesEAP()
        {
            if (btnEditarEAP.ClientVisible || btnVisualizarEAP.ClientVisible)
            {
                string codigoCrono = edicaoEAP ? codigoCronogramaReplanejamento : codigoCronogramaOficial;
                string urlEAP = "EdicaoEAP.aspx?IDProjeto=" + codigoProjeto + "&CCR=" + codigoCrono + "&CU=" + codigoUsuario + "&CE=" + codigoEntidadeUsuarioResponsavel + "&NP=" + nomeProjeto;
                if (utilizaNovaEAP == "S")
                    urlEAP = "../../GoJs/EAP/graficoEAP.aspx?IDProjeto=" + codigoProjeto + "&CCR=" + codigoCrono + "&CU=" + codigoUsuario + "&CE=" + codigoEntidadeUsuarioResponsavel + "&NP=" + nomeProjeto;
                string corpoEvento = string.Format(@"
                                             var Thiswidth = Math.max(0, window.top.document.documentElement.clientWidth) -75;
                                             var Thisheight = Math.max(0, window.top.document.documentElement.clientHeight) - 135;

                                             var myArguments = new Object();
   		                                     myArguments.param1 = '{0}';
                                             myArguments.param2 = 'ARG1';
                                             window.top.showModal('{1}&AM=ARG2&Altura=' + (Thisheight - 40), 'Edição EAP', null, null, ARG3, myArguments);"
                    , nomeProjeto
                    , urlEAP);
                
                if (edicaoEAP && eapBloqueadaEm != "")
                {
                    btnAbrirCronoBloqueadoEAP.ClientSideEvents.Click = "function(s, e) {" + corpoEvento.Replace("ARG1", " (VISUALIZAÇÃO) ").Replace("ARG2", "RO").Replace("ARG3", "''") + "pcInformacaoEAP.Hide();}";

                    corpoEvento = "pcInformacaoEAP.Show();";
                }

                string vs = "function(s, e) {" + corpoEvento.Replace("ARG1", " (VISUALIZAÇÃO) ").Replace("ARG2", "RO").Replace("ARG3", "funcaoPosModal") + "}";

                btnEditarEAP.ClientSideEvents.Click = "function(s, e) {" + corpoEvento.Replace("ARG1", "").Replace("ARG2", "RW").Replace("ARG3", "funcaoPosModalEdicao") + "}";
                btnVisualizarEAP.ClientSideEvents.Click = "function(s, e) {" + corpoEvento.Replace("ARG1", " (VISUALIZAÇÃO) ").Replace("ARG2", "RO").Replace("ARG3", "funcaoPosModal").Replace("'Edição EAP'", "'Visualização EAP'") + "}";
            }
        }

        private bool getPodeEditarEAP()
        {
            if (utilizaNovaEAP == "S")
            {
                string comandoSQL = string.Format(@"
               EXEC {0}.{1}.p_eap_BuscaCodigoEAP {2}
                ", cDados.getDbName(), cDados.getDbOwner(), codigoProjeto);


                DataSet ds = cDados.getDataSet(comandoSQL);

                if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    codigoCronogramaReplanejamento = dr["CronogramaReplanejamento"].ToString();
                    codigoCronogramaOficial = dr["CronogramaOficial"].ToString();
                    eapBloqueadaEm = string.Format("{0:dd/MM/yyyy}", dr["EAPBloqueadaEm"]);
                    eapBloqueadaPor = dr["EAPBloqueadaPor"].ToString();
                    edicaoEAP = dr["IndicaEAPEditavel"].ToString() == "S";

                    if (codigoCronogramaOficial == "" && codigoCronogramaReplanejamento == "")
                        codigoCronogramaReplanejamento = "-1";
                }
                else
                {
                    codigoCronogramaReplanejamento = "-1";
                    edicaoEAP = true;
                }
            }
            else
            {
                string comandoSQL = string.Format(@"
                SELECT  DataUltimaGravacaoDesktop 
                FROM    {0}.{1}.CronogramaProjeto 
                WHERE   CodigoProjeto = {2}
                  AND   DataUltimaGravacaoDesktop IS NOT NULL
                ", cDados.getDbName(), cDados.getDbOwner(), codigoProjeto);

                DataSet ds = cDados.getDataSet(comandoSQL);

                if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
                    edicaoEAP = false;
                else
                    edicaoEAP = true;

            }

            return edicaoEAP;
        }

        private void verificaPermissoesBotoes()
        {
            bool podeEditarEAP, podeVisualizarEAP, podeEditarCronograma, podeDesbloquearCronograma, naoPortifolio;

            podeVisualizarEAP = cDados.VerificaPermissaoUsuario(codigoUsuario, codigoEntidadeUsuarioResponsavel, codigoProjeto, "null", "PR", 0, "null", "PR_CnsEAP");
            podeEditarEAP = cDados.VerificaPermissaoUsuario(codigoUsuario, codigoEntidadeUsuarioResponsavel, codigoProjeto, "null", "PR", 0, "null", "PR_CadEAP");
            podeEditarCronograma = cDados.VerificaPermissaoUsuario(codigoUsuario, codigoEntidadeUsuarioResponsavel, codigoProjeto, "null", "PR", 0, "null", "PR_CadCrn");
            podeDesbloquearCronograma = cDados.VerificaPermissaoUsuario(codigoUsuario, codigoEntidadeUsuarioResponsavel, codigoProjeto, "null", "PR", 0, "null", "PR_DesBlq");
            naoPortifolio = getTipoObjetoPorCodigo(codigoProjeto) != "PTF";

            cDados.verificaPermissaoProjetoInativo(codigoProjeto, ref podeEditarEAP, ref podeEditarCronograma, ref podeDesbloquearCronograma);
            getPodeEditarEAP();

            btnEditarEAP.ClientVisible = naoPortifolio && podeEditarEAP && edicaoEAP && (codigoCronogramaReplanejamento != "" || utilizaNovaEAP == "N");
            btnVisualizarEAP.ClientVisible = podeVisualizarEAP && !edicaoEAP && (codigoCronogramaOficial != "" || utilizaNovaEAP == "N");

            btnEditarCronograma.ClientEnabled = podeEditarCronograma && naoPortifolio;
           // btnEditarCronograma2.ClientVisible = podeEditarCronograma && naoPortifolio;

            btnDesbloquear.ClientEnabled = podeDesbloquearCronograma;
        }

        public string getTipoObjetoPorCodigo(int codigoObjeto)
        {
            DataTable dt;
            string codigoTipoObjeto = "";

            string comandoSQL = string.Format(@"SELECT p.CodigoTipoProjeto, tp.IndicaTipoProjeto 
                                              FROM {0}.{1}.Projeto p
                                           INNER JOIN TipoProjeto tp on tp.CodigoTipoProjeto = p.CodigoTipoProjeto
                                             WHERE p.CodigoProjeto = {2}", cDados.getDbName(), cDados.getDbOwner(), codigoObjeto);
            dt = cDados.getDataSet(comandoSQL).Tables[0];
            if (cDados.DataTableOk(dt))
                codigoTipoObjeto = dt.Rows[0]["IndicaTipoProjeto"].ToString();

            return codigoTipoObjeto;
        }
    }

    
}
