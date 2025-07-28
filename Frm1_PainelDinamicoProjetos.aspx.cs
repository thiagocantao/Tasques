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

namespace BriskPtf._Projetos
{
public partial class _Projetos_Frm1_PainelDinamicoProjetos : System.Web.UI.Page
{
    dados cDados;

    //pega a data e hora atual do sistema
    string dataHora = DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + ".xml";
    public string grafico_xml1 = "", grafico_xml2 = "";
    int codigoUsuarioLogado, codigoEntidade;
    int codigoProjeto = -1;

    protected void Page_Init(object sender, EventArgs e)
    {
        DevExpress.Web.ASPxWebControl.SetIECompatibilityModeEdge();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
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

        codigoUsuarioLogado = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
        codigoEntidade = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());
        
        gvEntregas.Settings.ShowFilterRow = false;
        gvEntregas.SettingsPager.Mode = DevExpress.Web.GridViewPagerMode.ShowAllRecords;
        gvEntregas.SettingsLoadingPanel.Mode = DevExpress.Web.GridViewLoadingPanelMode.Disabled;
        lblNomeProjeto.Font.Size = new FontUnit("15pt");

        defineLarguraTela();

        if(Request.QueryString["CP"] != null && Request.QueryString["CP"].ToString() != "")
            codigoProjeto = int.Parse(Request.QueryString["CP"].ToString());

        lblNomeProjeto.Text = cDados.getNomeProjeto(codigoProjeto, "");
        carregaGridTarefas();
        geraGraficoCurvaSFinanceira();
        geraGraficoDesempenhoFisico();
        Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/FusionCharts.js""></script>"));
        this.TH(this.TS("FusionCharts"));

    }

    private void defineLarguraTela()
    {
        string ResolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString().ToString();
        int altura = int.Parse(ResolucaoCliente.Substring(ResolucaoCliente.IndexOf('x') + 1)); ;

        spl1.Height = altura - 100;
    }

    #region Grafico Desempenho Fisico

    private void geraGraficoDesempenhoFisico()
    {
        DataSet dsGrafico = cDados.getDesempenhoFisicoProjeto(codigoProjeto, "");

        DataTable dtGrafico = dsGrafico.Tables[0];

        dsGrafico = cDados.getIndicadorDesempenhoFisico("");

        DataTable dtIndicadores = dsGrafico.Tables[0];               

        string xml = "";

        string nomeGrafico = @"/ArquivosTemporarios/DesempenhoFisico_" + codigoUsuarioLogado + "_" + dataHora;

        xml = cDados.getGraficoDesempenhoFisico(dtGrafico, dtIndicadores, 9, "./ImageSaving/FusionChartsSave.aspx", "PercentualPrevisto", "PercentualReal", "", true);

        cDados.escreveXML(xml, nomeGrafico);

        grafico_xml1 = ".." + nomeGrafico;

        hfGeral.Set("grafico_xml1", grafico_xml1);
    }

    #endregion

    #region Grafico Curva S Financeira

    private void geraGraficoCurvaSFinanceira()
    {
        DataSet ds = cDados.getFinanceiroProjetoCurvaS(codigoProjeto, codigoEntidade, codigoUsuarioLogado);
        
        string xml = "";

        string nomeGrafico = @"/ArquivosTemporarios/CurvaSFinanceira_" + codigoUsuarioLogado + "_" + dataHora;

        xml = cDados.getGraficoProjetoCurvaSComLabel(ds.Tables[0], "Curva S Financeira", 9, 2, 10, 4, 25);

        cDados.escreveXML(xml, nomeGrafico);

        grafico_xml2 = ".." + nomeGrafico;

        hfGeral.Set("grafico_xml2", grafico_xml2);
    }

    #endregion

    #region Entregas

    private void carregaGridTarefas()
    {
        DataSet ds = cDados.getEntregasTarefasCliente(codigoProjeto, "AND TerminoReal IS NULL");

        if (cDados.DataSetOk(ds))
        {
            gvEntregas.DataSource = ds;

            gvEntregas.DataBind();
        }
    }

    #endregion
}

}
