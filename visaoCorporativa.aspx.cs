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
using System.IO;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;

namespace BriskPtf._Projetos
{
    public partial class _Projetos_visaoCorporativa : System.Web.UI.Page
    {
        dados cDados;
        public string alturaTela = "";

        protected void Page_PreInit(object sender, EventArgs e)
        {
            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            cDados = CdadosUtil.GetCdados(listaParametrosDados);
            cDados.aplicaTema(this.Page);
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            // =========================== Verifica se a sessão existe INICIO ========================
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
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"">var statusVC = 1;</script>"));
            // =========================== Verifica se a sessão existe FIM ========================

            int codigoUsuarioResponsavel = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidade = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

            if (!IsPostBack)
            {
                cDados.VerificaAcessoTela(this, codigoUsuarioResponsavel, codigoEntidade, codigoEntidade, "NULL", "EN", 0, "NULL", "EN_AcsPnlPrj");
            }


            //if (!IsPostBack)
            //    cDados.aplicaEstiloVisual(Page);

            defineLarguraTela();

            carregaComboStatus();

            if (!IsPostBack)
            {
                //if (cDados.getInfoSistema("CodigoStatus") != null)
                //    ddlStatus.Value = cDados.getInfoSistema("CodigoStatus").ToString();

                //cDados.setInfoSistema("CodigoStatus", ddlStatus.Items.Count > 0 ? ddlStatus.Value.ToString() : "-1");

                cDados.setInfoSistema("CodigoStatus", "3");

                cDados.excluiNiveisAbaixo(1);
                cDados.insereNivel(1, this);

                Master.verificaAcaoFavoritos(true, lblTituloTela.Text, "PNLPRJ", "ENT", -1, Resources.traducao.adicionar_aos_favoritos);
            }
            Master.geraRastroSite();
            this.Title = cDados.getNomeSistema();

            DataSet dsParam = cDados.getParametrosSistema(codigoEntidade, "ModeloVCProjetos", "mostrarFiltroFinanceiroProjetos");

            string modeloVC = "./VisaoCorporativa/visaoCorporativa_01.aspx";

            bool mostrarFiltroFinanceiro = false;

            if (cDados.DataSetOk(dsParam) && cDados.DataTableOk(dsParam.Tables[0]))
            {
                if (dsParam.Tables[0].Rows[0]["ModeloVCProjetos"].ToString() != "")
                    modeloVC = "./VisaoCorporativa/" + dsParam.Tables[0].Rows[0]["ModeloVCProjetos"].ToString() + ".aspx";

                mostrarFiltroFinanceiro = dsParam.Tables[0].Rows[0]["mostrarFiltroFinanceiroProjetos"].ToString() == "S";
            }

            if (mostrarFiltroFinanceiro)
            {
                modeloVC = modeloVC + "?Financeiro=" + (ddlFinanceiro.Value.ToString() == "A" ? DateTime.Now.Year : -1);
                ddlFinanceiro.ClientVisible = true;
                lblFinanceiro.ClientVisible = true;
                btnSelecionar.ClientVisible = true;
            }
            else
            {
                modeloVC = modeloVC + "?Financeiro=-1";
                ddlFinanceiro.ClientVisible = false;
                lblFinanceiro.ClientVisible = false;
                btnSelecionar.ClientVisible = false;
            }

            hfVisaoCorporativa.Set("UrlVC", modeloVC);

            callBackVC.JSProperties["cp_UrlVC"] = modeloVC;

            int codigoCarteira = int.Parse(cDados.getInfoSistema("CodigoCarteira").ToString());

            DataSet dsProjetos = cDados.f_getProjetosUsuario(codigoUsuarioResponsavel, codigoEntidade, codigoCarteira);

            string codigosProjetos = "-1";

            if (cDados.DataSetOk(dsProjetos) && cDados.DataTableOk(dsProjetos.Tables[0]))
            {
                foreach (DataRow dr in dsProjetos.Tables[0].Rows)
                {
                    codigosProjetos += "," + dr["CodigoProjeto"];
                }
            }

            cDados.setInfoSistema("CodigosProjetosUsuario", codigosProjetos);

            DataSet dsParamImprimePainelProj = cDados.getParametrosSistema(codigoEntidade, "mostraImpressaoPainelProjetos");

            bool imprimePainelProjetos = false;
            if (cDados.DataSetOk(dsParamImprimePainelProj) && cDados.DataTableOk(dsParamImprimePainelProj.Tables[0]))
            {
                imprimePainelProjetos = dsParamImprimePainelProj.Tables[0].Rows[0]["mostraImpressaoPainelProjetos"].ToString() == "S";
            }
            imgExportaParaPDF.Visible = imprimePainelProjetos;


        }

        private void defineLarguraTela()
        {
            string ResolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString().ToString();
            int altura = int.Parse(ResolucaoCliente.Substring(ResolucaoCliente.IndexOf('x') + 1)); ;

            alturaTela = (altura - 210).ToString() + "px";
        }

        protected void callBackVC_Callback(object source, DevExpress.Web.CallbackEventArgs e)
        {
            //cDados.setInfoSistema("CodigoStatus", ddlStatus.Items.Count > 0 ? ddlStatus.Value.ToString() : "-1");
        }

        private void carregaComboStatus()
        {
            DataSet dsStatus = cDados.getStatus(" AND IndicaAcompanhamentoExecucao = 'S'");

            if (cDados.DataSetOk(dsStatus))
            {
                ddlStatus.DataSource = dsStatus;

                ddlStatus.TextField = "DescricaoStatus";

                ddlStatus.ValueField = "CodigoStatus";

                ddlStatus.DataBind();

                ListEditItem lei = new ListEditItem(Resources.traducao.todos, "-1");

                ddlStatus.Items.Insert(0, lei);

                if (!IsPostBack && cDados.DataTableOk(dsStatus.Tables[0]))
                    ddlStatus.Value = "3";
                else
                    ddlStatus.SelectedIndex = 0;
            }
        }

        protected void ImageButton1_Click(object sender, ImageClickEventArgs e)
        {
            string anoFinanceiro = "";
            anoFinanceiro = ddlFinanceiro.Value != null && ddlFinanceiro.Value.ToString() + "" != "" ? ddlFinanceiro.Value.ToString() : "";


            rel_ImpressaoPainelProjetos relIPP = new rel_ImpressaoPainelProjetos(anoFinanceiro);

            if (EhParametroDeRiscoUnigest())
            {
                relGanttProjetos_unigest relGP_unigest = new relGanttProjetos_unigest();

                int codigoCarteira = int.Parse(cDados.getInfoSistema("CodigoCarteira").ToString());
                string descricaoCarteira = "";
                DataSet ds = cDados.getCarteirasDeProjetos(" and cp.CodigoCarteira = " + codigoCarteira);
                if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
                {
                    descricaoCarteira = ds.Tables[0].Rows[0]["NomeCarteira"].ToString();
                }
                string nomeEntidade = "";
                DataSet dsNomeEntidade = cDados.getUnidade(" and CodigoUnidadeNegocio = " + cDados.getInfoSistema("CodigoEntidade").ToString());
                if (cDados.DataSetOk(dsNomeEntidade) && cDados.DataTableOk(dsNomeEntidade.Tables[0]))
                {
                    nomeEntidade = dsNomeEntidade.Tables[0].Rows[0]["NomeUnidadeNegocio"].ToString();
                }
                relGP_unigest.pTitulo.Value = nomeEntidade;

                relRiscosQuestoesProjetoCarteira_unigest relRQPC_unigest = new relRiscosQuestoesProjetoCarteira_unigest();

                relRQPC_unigest.pCodigoCarteira.Value = codigoCarteira;

                relRQPC_unigest.pCodigoProjeto.Value = "null";

                relRQPC_unigest.pIndicaRetornaTodosChar.Value = "";

                relIPP.CreateDocument();
                relGP_unigest.CreateDocument();

                relRQPC_unigest.CreateDocument();

                DevExpress.XtraPrinting.Page[] pagina_relIPP = relIPP.Pages.ToArray();


                DevExpress.XtraPrinting.Page[] pagina_relGP_unigest = relGP_unigest.Pages.ToArray();


                DevExpress.XtraPrinting.Page[] pagina_relRQPC_unigest = relRQPC_unigest.Pages.ToArray();

                for (int i = 0; i < pagina_relIPP.Length; i++)
                {
                    relIPP.Pages.Add(pagina_relIPP[i]);
                }

                for (int i = 0; i < pagina_relGP_unigest.Length; i++)
                {
                    relIPP.Pages.Add(pagina_relGP_unigest[i]);
                }

                for (int i = 0; i < pagina_relRQPC_unigest.Length; i++)
                {
                    relIPP.Pages.Add(pagina_relRQPC_unigest[i]);
                }


            }
            else
            {
                relGanttProjetos relGP = new relGanttProjetos();

                int codigoCarteira = int.Parse(cDados.getInfoSistema("CodigoCarteira").ToString());
                string descricaoCarteira = "";
                DataSet ds = cDados.getCarteirasDeProjetos(" and cp.CodigoCarteira = " + codigoCarteira);
                if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
                {
                    descricaoCarteira = ds.Tables[0].Rows[0]["NomeCarteira"].ToString();
                }
                string nomeEntidade = "";
                DataSet dsNomeEntidade = cDados.getUnidade(" and CodigoUnidadeNegocio = " + cDados.getInfoSistema("CodigoEntidade").ToString());
                if (cDados.DataSetOk(dsNomeEntidade) && cDados.DataTableOk(dsNomeEntidade.Tables[0]))
                {
                    nomeEntidade = dsNomeEntidade.Tables[0].Rows[0]["NomeUnidadeNegocio"].ToString();
                }
                relGP.pTitulo.Value = nomeEntidade;

                relRiscosQuestoesProjetoCarteira relRQPC = new relRiscosQuestoesProjetoCarteira();

                relRQPC.pCodigoCarteira.Value = codigoCarteira;

                relRQPC.pCodigoProjeto.Value = "null";

                relRQPC.pIndicaRetornaTodosChar.Value = "";

                relIPP.CreateDocument();
                relGP.CreateDocument();

                relRQPC.CreateDocument();

                DevExpress.XtraPrinting.Page[] pagina_relIPP = relIPP.Pages.ToArray();


                DevExpress.XtraPrinting.Page[] pagina_relGP_unigest = relGP.Pages.ToArray();


                DevExpress.XtraPrinting.Page[] pagina_relRQPC_unigest = relRQPC.Pages.ToArray();

                for (int i = 0; i < pagina_relIPP.Length; i++)
                {
                    relIPP.Pages.Add(pagina_relIPP[i]);
                }

                for (int i = 0; i < pagina_relGP_unigest.Length; i++)
                {
                    relIPP.Pages.Add(pagina_relGP_unigest[i]);
                }

                for (int i = 0; i < pagina_relRQPC_unigest.Length; i++)
                {
                    relIPP.Pages.Add(pagina_relRQPC_unigest[i]);
                }
            }

            ExportReport(relIPP, "relatorio", "pdf", false);
        }

        private bool EhParametroDeRiscoUnigest()
        {
            bool retorno = false;

            string IndicaTelaRiscoUNIGEST = "N";
            DataSet ds = cDados.getParametrosSistema("IndicaTelaRiscoUNIGEST");
            if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
            {
                IndicaTelaRiscoUNIGEST = ds.Tables[0].Rows[0]["IndicaTelaRiscoUNIGEST"].ToString().Trim();
            }
            retorno = IndicaTelaRiscoUNIGEST == "S";
            return retorno;
        }

        public void ExportReport(XtraReport report, string fileName, string fileType, bool inline)
        {
            MemoryStream stream = new MemoryStream();

            Response.Clear();

            if (fileType == "xls")
            {
                XlsExportOptionsEx x = new XlsExportOptionsEx();
                x.ExportType = DevExpress.Export.ExportType.WYSIWYG;
                report.ExportToXls(stream, x);
            }

            if (fileType == "pdf")
                report.ExportToPdf(stream);
            if (fileType == "rtf")
                report.ExportToRtf(stream);
            if (fileType == "csv")
                report.ExportToCsv(stream);

            Response.ContentType = "application/" + fileType;
            Response.AddHeader("Accept-Header", stream.Length.ToString());
            Response.AddHeader("Content-Disposition", string.Format("{0}; filename={1}.{2}",
                (inline ? "Inline" : "Attachment"), fileName, fileType));
            Response.AddHeader("Content-Length", stream.Length.ToString());
            //Response.ContentEncoding = System.Text.Encoding.Default;
            Response.BinaryWrite(stream.ToArray());
            Response.End();

        }
    }

}
