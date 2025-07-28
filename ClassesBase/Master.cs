using DevExpress.Web;
using System;
using System.Data;
using System.Xml;

/// <summary>
/// Summary description for Master
/// </summary>


namespace BriskPtf
{
public class Master : System.Web.UI.MasterPage
{
    public string mostrarAcaoFavoritos;
    dados cDados = CdadosUtil.GetCdados(null);

    public virtual void geraRastroSite()
    {
        if (Master != null && Master.FindControl("lblCaminho") != null)
        {
            ASPxLabel lblCaminho = (Master.FindControl("lblCaminho") as ASPxLabel);

            int codigoUsuarioResponsavel = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeUsuarioResponsavel = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

            string caminho = Session["NomeArquivoNavegacao"] + "";

            if (Session["NomeArquivoNavegacao"] != null && caminho != "")
            {
                try
                {
                    XmlDocument doc = new XmlDocument();

                    doc.Load(caminho);

                    int i = 0;

                    for (i = 0; i < doc.ChildNodes[1].ChildNodes.Count; i++)
                    {
                        XmlNode no = doc.SelectSingleNode(String.Format("/caminho/N[id={0}]", i));
                        if (no != null)
                        {
                            lblCaminho.Text += String.Format(" /<a style='font-size:7pt;' href='{0}?{2}'>{1}</a>"
                                , cDados.getPathSistema() + no.SelectSingleNode("./url").InnerText
                                , no.SelectSingleNode("./nome").InnerText
                                , no.SelectSingleNode("./parametros").InnerText);
                        }
                    }
                }
                catch
                {
                    Response.RedirectLocation = cDados.getPathSistema() + "po_autentica.aspx";
                }
            }
            else
            {
                string nomeArquivo = "/ArquivosTemporarios/xml_Caminho" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + "_" + codigoUsuarioResponsavel + ".xml"; ;
                string urlDestino = "";
                string nomeTela = "";

                DataSet ds = cDados.getURLTelaInicialUsuario(codigoEntidadeUsuarioResponsavel.ToString(), codigoUsuarioResponsavel.ToString());

                if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
                {
                    urlDestino = ds.Tables[0].Rows[0]["TelaInicial"].ToString();
                    nomeTela = "Meu Painel de Bordo";
                }

                string xml = string.Format(@"<caminho>
	                                            <N>
		                                            <id>0</id>
                                                    <nivel>0</nivel>
		                                            <url>{0}</url>
		                                            <nome>{1}</nome>
		                                            <parametros></parametros>
	                                            </N>
                                            </caminho>", urlDestino.Replace("~/", ""), nomeTela);

                Session["NomeArquivoNavegacao"] = Request.PhysicalApplicationPath + nomeArquivo;

                cDados.escreveXML(xml, nomeArquivo);
            }
        }
    }

    public virtual void verificaAcaoFavoritos(bool mostrarIcone, string nomeTela, string iniciaisMenu, string iniciaisTipoObjeto, int codigoObjetoAssociado, string toolTip)
    {
        if (Master != null && Master.FindControl("imgAcaoFavoritos") != null)
        {
            ASPxImage imgAcaoFavoritos = (Master.FindControl("imgAcaoFavoritos") as ASPxImage);
            ASPxHiddenField hfFavoritos = (Master.FindControl("hfFavoritos") as ASPxHiddenField);
            ASPxTextBox txtNomeFavorito = (Master.FindControl("txtNomeFavorito") as ASPxTextBox);

            int codigoUsuarioLogado = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeLogada = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

            if (mostrarIcone)
            {
                imgAcaoFavoritos.ClientVisible = true;

                if (codigoObjetoAssociado == -1)
                    codigoObjetoAssociado = codigoEntidadeLogada;

                bool existeFavorito = cDados.verificaExistenciaFavoritosUsuario(codigoEntidadeLogada, codigoUsuarioLogado, iniciaisMenu, iniciaisTipoObjeto, codigoObjetoAssociado);

                if (existeFavorito)
                {
                    imgAcaoFavoritos.ToolTip = "Remover dos Favoritos";
                    hfFavoritos.Set("TipoEdicaoFavorito", "E");
                    imgAcaoFavoritos.ImageUrl = "~/imagens/principal/favoritos_RMV.png";
                    imgAcaoFavoritos.ClientSideEvents.Click = @"function(s, e) {callbackFavoritos.PerformCallback();}";
                }
                else
                {
                    imgAcaoFavoritos.ToolTip = toolTip;
                    hfFavoritos.Set("TipoEdicaoFavorito", "I");
                    imgAcaoFavoritos.ImageUrl = "~/imagens/principal/favoritos_ADD.png";
                    imgAcaoFavoritos.ClientSideEvents.Click = "function(s, e) {try{pcNovoFavorito.Show();}catch(e){}}";
                }

                txtNomeFavorito.Text = nomeTela.Length > 100 ? nomeTela.Substring(0, 95) + "..." : nomeTela;
                hfFavoritos.Set("NomeTelaReferencia", nomeTela.Length > 100 ? nomeTela.Substring(0, 95) + "..." : nomeTela);
                hfFavoritos.Set("URL", Request.AppRelativeCurrentExecutionFilePath.ToString() + "?" + Request.QueryString.ToString());
                hfFavoritos.Set("IniciaisMenu", iniciaisMenu);
                hfFavoritos.Set("IniciaisTipoObjeto", iniciaisTipoObjeto);
                hfFavoritos.Set("CodigoObjetoAssociado", codigoObjetoAssociado);
                mostrarAcaoFavoritos = "block";
            }
            else
            {
                imgAcaoFavoritos.ClientVisible = false;
                mostrarAcaoFavoritos = "none";
            }
        }
    }

}
}