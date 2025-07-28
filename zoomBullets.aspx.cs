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
public partial class _Projetos_zoomBullets : System.Web.UI.Page
{
    public string grafico1_swf = "";
    public string grafico1_xml = "";
    public string grafico2_swf = "";
    public string grafico2_xml = "";
    public string grafico3_swf = "";
    public string grafico3_xml = "";

    //Mensagens de erro para os gráficos
    public string msgErro = "?LoadDataErrorText=Sem Informações a Apresentar";
    public string msgNoData = "&ChartNoDataText=Sem Informações a Apresentar";
    public string msgInvalid = "&InvalidXMLText=Sem Informações a Apresentar";
    public string desenhando = "&PBarLoadingText=Gerando imagem...";
    public string msgLoading = "&XMLLoadingText=Carregando...";

    public char mostrarReceita = 'S';
    public char mostrarEsforco = 'S';
    public int alturaGrafico = 145;

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
 
        //Atribui os valores recebidos como parametro às variáveis para carregar os gráficos
        grafico1_swf = "../Flashs/" + Request.QueryString["SWF1"].ToString();
        grafico1_xml = "../" + Request.QueryString["XML1"].ToString() + msgErro + msgNoData + msgInvalid + desenhando + msgLoading;
        grafico2_swf = "../Flashs/" + Request.QueryString["SWF2"].ToString();
        grafico2_xml = "../" + Request.QueryString["XML2"].ToString() + msgErro + msgNoData + msgInvalid + desenhando + msgLoading;
        grafico3_swf = "../Flashs/" + Request.QueryString["SWF3"].ToString();
        grafico3_xml = "../" + Request.QueryString["XML3"].ToString() + msgErro + msgNoData + msgInvalid + desenhando + msgLoading;

        DataSet ds1 = cDados.getParametrosSistema("tituloPaginasWEB");
        if(cDados.DataSetOk(ds1) && cDados.DataTableOk(ds1.Tables[0]))
        {
            Page.Title = ds1.Tables[0].Rows[0][0].ToString() + " - Zoom";
        }
        //Atribui o valor recebido como parametro ao título do gráfico
        //lbl_TituloGrafico.Text = Request.QueryString["TIT"].ToString();
    }
}

}
