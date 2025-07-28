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
using System.IO;
using System.Web.Hosting;

namespace BriskPtf._Projetos
{
public partial class _Projetos_sobre : System.Web.UI.Page
{
    
    dados cDados;
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
        lblNomeCliente.Text = "Licenciado para: CDIS Informática LTDA";

        DataSet ds = cDados.getParametrosSistema("nomeEmpresa");
        if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
        {
            lblNomeCliente.Text = "Licenciado para: " + ds.Tables[0].Rows[0]["nomeEmpresa"].ToString();
        }

        try
        {
            FileInfo fInfo = new FileInfo(HostingEnvironment.ApplicationPhysicalPath + @"bin\App_Code.dll");
            lblAtualizacao.Text = string.Format("{1}: {0:dd/MM/yyyy hh:mm}", fInfo.LastWriteTime, Resources.traducao._ltima_atualiza__o);
       

            
        DataSet ds1 = cDados.getParametrosSistema("tituloPaginasWEB");
        if(cDados.DataSetOk(ds1) && cDados.DataTableOk(ds1.Tables[0]))
        {
            Page.Title = ds1.Tables[0].Rows[0][0].ToString();
            spnTitulo.InnerText = Page.Title;
        }
        }
        catch
        {
            lblAtualizacao.Text = "";
        }

    }
}

}
