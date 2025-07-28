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

namespace BriskPtf._Projetos
{
public partial class _Projetos_zoom : System.Web.UI.Page
{
    public string grafico_swf = "";
    public string grafico_xml = "";
    public string tipoGrafico = "";

    //Mensagens de erro para os gráficos
    public string msgErro = "?LoadDataErrorText=Sem Informações a Apresentar";
    public string msgNoData = "&ChartNoDataText=Sem Informações a Apresentar";
    public string msgInvalid = "&InvalidXMLText=Sem Informações a Apresentar";
    public string desenhando = "&PBarLoadingText=Gerando o gráfico...";
    public string msgLoading = "&XMLLoadingText=Carregando...";

    protected void Page_Init(object sender, EventArgs e)
    {
        DevExpress.Web.ASPxWebControl.SetIECompatibilityModeEdge();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        dados cDados;

        System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

        listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
        listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

        cDados = CdadosUtil.GetCdados(listaParametrosDados);

        // a pagina não pode ser armazenada no cache
        Response.Cache.SetCacheability(HttpCacheability.NoCache);

        //Atribui às variáveis os valores passados como parametro para carregar o gráfico
        grafico_swf = Request.QueryString["SWF"].ToString();
        grafico_xml = ".." + Request.QueryString["XML"].ToString() + msgErro + msgNoData + msgInvalid + desenhando + msgLoading;
        tipoGrafico = Request.QueryString["Tipo"].ToString();

        lbl_TituloGrafico.Visible = false;

        DataSet ds1 = cDados.getParametrosSistema("tituloPaginasWEB");
        if (cDados.DataSetOk(ds1) && cDados.DataTableOk(ds1.Tables[0]))
        {
            Page.Title = ds1.Tables[0].Rows[0][0].ToString() + " - Zoom";
        }
    }
}

}
