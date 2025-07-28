using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using DevExpress.Web;
using System.IO;

namespace BriskPtf._Projetos
{
    public partial class _Projetos_analiseDeFluxoPorPeriodo : System.Web.UI.Page
    {
        dados cDados;

        private int codigoUsuarioResponsavel;
        public int codigoEntidadeUsuarioResponsavel;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            cDados = CdadosUtil.GetCdados(listaParametrosDados);
            cDados.aplicaTema(this.Page);
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            dsAnexos.ConnectionString = cDados.classeDados.getStringConexao();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            HeaderOnTela();
            codigoUsuarioResponsavel = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            codigoEntidadeUsuarioResponsavel = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

            if (!IsPostBack)
            {
                bool bPodeAcessarTela = cDados.VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "EN_AnlFlxPrd");

                if (bPodeAcessarTela == false)
                    cDados.RedirecionaParaTelaSemAcesso(this);
            }

            this.Title = cDados.getNomeSistema();

            if (!IsPostBack)
            {
                populaComboCarteiras();


                dtDe.Value = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
                dteAte.Value = DateTime.Now;
            }

            populaGridFluxos();
            defineAlturaTela();
        }

        private void HeaderOnTela()
        {
            // a pagina não pode ser armazenada no cache.
            Response.Cache.SetCacheability(HttpCacheability.NoCache); // Ok

            // inclui o arquivo de scripts desta tela
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"">var pastaImagens = ""../../imagens"";</script>"));
            Header.Controls.Add(cDados.getLiteral(@"<link href=""../../estilos/cdisEstilos.css"" rel=""stylesheet"" type=""text/css"" />"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/analiseDeFluxoPorPeriodo.js""></script>"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/barraNavegacao.js""></script>"));
            this.TH(this.TS("barraNavegacao", "analiseDeFluxoPorPeriodo"));
        }

        private void defineAlturaTela()
        {
            string resolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString();
            // Calcula a altura da tela
            int largura = 0;
            int altura = 0;
            cDados.getLarguraAlturaTela(resolucaoCliente, out largura, out altura);

            gvDados.Settings.VerticalScrollableHeight = altura - 400;
        }

        private void populaComboCarteiras()
        {
            cDados.defineConfiguracoesComboCarteiras(cmbCarteiras, IsPostBack, codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, false);
            //cmbCarteiras.  defineConfiguracoesComboCarteiras 
        }



        private void populaGridFluxos()
        {
            int codigoCarteira = int.Parse(cmbCarteiras.Value.ToString());

            string comandoSQL = string.Format(
                @"BEGIN
                  DECLARE @CodigoEntidade Int,
                          @CodigoCarteira Int,
                          @CodigoUsuario Int,
                          @DataInicio as datetime,
                          @DataTermino as datetime,
                          
                          @CodigoTipoAssociacao SmallInt
  
                  /* --- Parâmetros da consulta ---*/
                  SET @CodigoEntidade = {0}
  
                  SET @CodigoUsuario = {1}
  
                  SET @CodigoCarteira = {2}
                  
                  SET @DataInicio = convert(datetime,'{3}',103)
                  SET @DataTermino = convert(datetime,'{4}',103)

                  /* --- Fim dos parâmetros -------*/
  
                  /* Obtém o código do tipo de associação para FORMULÁRIO */
                  SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao
                    FROM TipoAssociacao 
                   WHERE IniciaisTipoAssociacao = 'FO'
  
                  SELECT DISTINCT 
                         @CodigoCarteira AS CodigoCarteira, 
                         p.CodigoProjeto, 
                         p.NomeProjeto, 
                         un.NomeUnidadeNegocio,
                         (SELECT TOP 1 uns.NomeUnidadeNegocio 
                            FROM UnidadeNegocio AS uns 
                           WHERE uns.CodigoUnidadeNegocio = un.CodigoUnidadeNegocioSuperior) AS NomeUnidadeNegocioSuperior, 
                         f.NomeFluxo + ' - ' + p.NomeProjeto AS NomeFluxo,
                         ewf.NomeEtapaWf AS EtapaAtual, 
                         YEAR((SELECT Min(eiw.DataInicioEtapa)
								  FROM EtapasInstanciasWf AS eiw INNER JOIN
									   Workflows AS w ON (w.CodigoWorkflow = eiw.CodigoWorkflow)
								 WHERE eiw.CodigoWorkflow = iw.CodigoWorkflow
								   AND eiw.CodigoInstanciaWf = iw.CodigoInstanciaWf
								   AND eiw.CodigoEtapaWf  <> w.CodigoEtapaInicial)) AS AnoInicio, 
                         MONTH((SELECT Min(eiw.DataInicioEtapa)
								  FROM EtapasInstanciasWf AS eiw INNER JOIN
									   Workflows AS w ON (w.CodigoWorkflow = eiw.CodigoWorkflow)
								 WHERE eiw.CodigoWorkflow = iw.CodigoWorkflow
								   AND eiw.CodigoInstanciaWf = iw.CodigoInstanciaWf
								   AND eiw.CodigoEtapaWf  <> w.CodigoEtapaInicial)) AS MesInicio,
                         w.CodigoWorkflow, 
                         iw.CodigoInstanciaWf,
                         CASE WHEN NomeFluxo IS NULL AND EtapaAtual IS NULL 
                              THEN 'Sem fluxo'
                              WHEN EtapaAtual IS NULL 
                              THEN 'Finalizado'
                         ELSE 'Em andamento' 
                         END AS Status,
                            (SELECT COUNT(DISTINCT a.CodigoAnexo)
                               FROM FormulariosInstanciasWorkflows AS fiw 
                         INNER JOIN AnexoAssociacao AS aa ON (aa.CodigoObjetoAssociado = fiw.CodigoFormulario AND aa.CodigoTipoAssociacao = @CodigoTipoAssociacao) 
                         INNER JOIN Anexo a on a.CodigoAnexo = aa.CodigoAnexo                                                                                                      
                              WHERE fiw.CodigoWorkflow = w.CodigoWorkflow
                                AND fiw.CodigoInstanciaWf = iw.CodigoInstanciaWf) as QtdeAnexos,
                         (SELECT NomeUsuario FROM Usuario 
                           WHERE CodigoUsuario = iw.IdentificadorUsuarioCriadorInstancia) as nomeResponsavelFluxo,
                         (SELECT Min(eiw.DataInicioEtapa)
								  FROM EtapasInstanciasWf AS eiw INNER JOIN
									   Workflows AS w ON (w.CodigoWorkflow = eiw.CodigoWorkflow)
								 WHERE eiw.CodigoWorkflow = iw.CodigoWorkflow
								   AND eiw.CodigoInstanciaWf = iw.CodigoInstanciaWf
								   AND eiw.CodigoEtapaWf  <> w.CodigoEtapaInicial) AS DataInicioInstancia, -- Foi solicitado para trazer a primeira etapa em que houve interação do usuário
                                 iw.DataTerminoInstancia
                                                               
                        FROM f_GetProjetosUsuario(@CodigoUsuario,@CodigoEntidade, @CodigoCarteira) AS cp 
                  INNER JOIN Projeto AS p ON (p.CodigoProjeto = cp.CodigoProjeto)             
                  INNER JOIN UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) 
                  INNER JOIN Usuario AS u ON (u.CodigoUsuario = p.CodigoGerenteProjeto) 
                   LEFT JOIN InstanciasWorkflows AS iw ON (iw.IdentificadorProjetoRelacionado = p.CodigoProjeto
                                                       AND iw.DataCancelamentoInstancia IS NULL
                                                       AND (convert(datetime,iw.DataInicioInstancia,103) BETWEEN @DataInicio and @DataTermino)) 
                   LEFT JOIN EtapasWf AS ewf ON (ewf.CodigoEtapaWf = iw.EtapaAtual
                                             AND ewf.CodigoWorkflow = iw.CodigoWorkflow) 
                   LEFT JOIN Workflows AS w ON (w.CodigoWorkflow = iw.CodigoWorkflow) 
                   LEFT JOIN Fluxos AS f ON (f.CodigoFluxo = w.CodigoFluxo) 
                       WHERE p.CodigoEntidade = @CodigoEntidade  
                       ORDER BY  Status, NomeUnidadeNegocio, NomeProjeto                                                                                      
                END", codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, codigoCarteira, dtDe.Text, dteAte.Text);
            DataSet ds = cDados.getDataSet(comandoSQL);
            gvDados.DataSource = ds.Tables[0];
            gvDados.DataBind();

            if (IsCallback && IsPostBack)
                gvDados.DetailRows.CollapseAllRows();
        }

        /// <summary>
        ///  determina se o botão "+" para mostrar a linha de detalhes será mostrado. - Se não existir anexos não pode mostrar o botão
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gvDados_DetailRowGetButtonVisibility(object sender, ASPxGridViewDetailRowButtonEventArgs e)
        {
            string qtdeAnexos = gvDados.GetRowValues(e.VisibleIndex, "QtdeAnexos").ToString();
            if (qtdeAnexos == "0")
                e.ButtonState = GridViewDetailRowButtonState.Hidden;
        }

        protected void gvDados_DetailRowExpandedChanged(object sender, DevExpress.Web.ASPxGridViewDetailRowEventArgs e)
        {
            if (e.Expanded)
            {
                // o SELECT para buscar os anexos está dentro do componente "dsAnexos"
                string CodigoWorkflow = gvDados.GetRowValues(e.VisibleIndex, "CodigoWorkflow").ToString();
                string CodigoInstanciaWf = gvDados.GetRowValues(e.VisibleIndex, "CodigoInstanciaWf").ToString();
                dsAnexos.SelectParameters["CodigoWorkflow"].DefaultValue = CodigoWorkflow;
                dsAnexos.SelectParameters["CodigoInstanciaWf"].DefaultValue = CodigoInstanciaWf;

                ASPxGridView grid = (ASPxGridView)gvDados.FindDetailRowTemplateControl(e.VisibleIndex, "gvAnexos");
               
            }
        }

        // Retorna para a propriedade "ClientInstanceName" uma string contendo o código do anexo
        public string getNomeBotalDownload()
        {
            return "btnDownLoad_" + Eval("CodigoAnexo").ToString();
        }

        protected void btnDownLoad_Click(object sender, EventArgs e)
        {
            string CodigoAnexo = (sender as ASPxButton).ClientInstanceName.Substring(12);
            cDados.download(int.Parse(CodigoAnexo), null, Page, Response, Request, true);
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            gvExporter.WriteXlsToResponse(new DevExpress.XtraPrinting.XlsExportOptionsEx() { ExportType = DevExpress.Export.ExportType.WYSIWYG });
        }

        protected void gvExporter_RenderBrick(object sender, ASPxGridViewExportRenderingEventArgs e)
        {
            if (e.RowType == GridViewRowType.Group)
            {
                if (e.Text.IndexOf(':') != -1)
                {
                    string DescricaoColuna = e.Text.Substring(0, e.Text.IndexOf(':'));
                    string strValue = System.Text.RegularExpressions.Regex.Replace(DescricaoColuna + ": " + e.Value, @"<[^>]*>", " ");
                    e.TextValue = strValue;
                    e.Text = strValue;
                }
            }
        }
    }
}
