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
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Collections.Generic;

namespace BriskPtf._Projetos
{
    public partial class _Projetos_resumoProjetos : System.Web.UI.Page
    {
        dados cDados;

        private int codigoUsuarioResponsavel;
        public int codigoEntidadeUsuarioResponsavel;
        public string alturaTabela, displayLaranja = "", displayUnidadeAtendimento = "", displayGerenteProjeto = "", displayCategoria = "", displayStatus = "", displayCR = "", displaySuspensos = "N", displayCancelados = "N", displayEncerrados = "N";

        DataSet dsProjetos;
        DataSet dsParametros;

        bool mostrarBoletins = true;

        public string estiloFooter = "dxtlControl dxtlFooter";

        public string script = "";

        public List<String> itensLegenda = new List<String>();

        private int codigoCarteiraTela, codigoCarteiraListaProcessos;
        bool bPodeAcessarTela, bPodeIncluirProjetos;

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
            if (cDados.getInfoSistema("ResolucaoCliente") == null)
                Response.Redirect("~/index.aspx");

            codigoUsuarioResponsavel = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            codigoEntidadeUsuarioResponsavel = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());

            inicializaCombosFiltros();
            this.Title = cDados.getNomeSistema();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Header.Controls.Add(cDados.getLiteral(@"<link href=""../estilos/cdisEstilos.css"" rel=""stylesheet"" type=""text/css"" />"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/ListaProjetos.js""></script>"));
            this.TH(this.TS("resumoProjetos", "ListaProjetos"));
            string cssPostfix = "", cssPath = "";

            //cDados.aplicaEstiloVisual(Page);
            cDados.getVisual(cDados.getInfoSistema("IDEstiloVisual").ToString(), ref cssPath, ref cssPostfix);

            bPodeAcessarTela = cDados.VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "AcsLstPrj");

            /// se houver algum unidade em que o usuário possa incluir projetos
            bPodeIncluirProjetos = cDados.VerificaAcessoEmAlgumObjeto(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "UN", "UN_IncPrj");

            bPodeAcessarTela = bPodeAcessarTela || bPodeIncluirProjetos;

            if (bPodeAcessarTela == false)
                cDados.RedirecionaParaTelaSemAcesso(this);

            if (cssPostfix != "")
                estiloFooter = "dxtlControl_" + cssPostfix + " dxtlFooter_" + cssPostfix;

            if (!IsPostBack)
            {
                if (cDados.getInfoSistema("CodigoStatus") != null)
                    ddlStatus.Value = cDados.getInfoSistema("CodigoStatus").ToString();

                verificaFiltrosParametro();
                //Foi escolhido o nível 1 devido ao momento ao acessar a e qual nível a página está. Página Resumo já tem o seguinte mapeanento: (0)Meu Painel de Bordo/(1)Initiatives/(1)Limpa o seu nível respectivo e coloca a página em execução.
                cDados.excluiNiveisAbaixo(1);
                cDados.insereNivel(1, this);
                Master.geraRastroSite();
                Master.verificaAcaoFavoritos(true, "Projetos da Instituição", "LPROJ", "ENT", -1, "Adicionar Lista aos Favoritos");
            }


            DataSet ds = cDados.getDefinicaoUnidade(codigoEntidadeUsuarioResponsavel);
            if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
            {
                string definicaoUnidade = ds.Tables[0].Rows[0]["DescricaoTipoUnidadeNegocio"].ToString();
                lblUnidades.Text = definicaoUnidade + ":";
                ddlUnidades.Columns[1].Caption = definicaoUnidade;
                hfGeral.Set("definicaoUnidade", definicaoUnidade);
            }

            lblStatus.Text = Resources.traducao.resumoProjetos_status_;
            lblCR.Text = Resources.traducao.resumoProjetos_CR_;

            lblUnidadeAtendimento.Text = cDados.defineLabelUnidadeAtendimento();
            ddlUnidadeAtendimento.Columns[1].Caption = lblUnidadeAtendimento.Text;
            hfGeral.Set("definicaoUnidadeAtendimento", lblUnidadeAtendimento.Text);

            dsParametros = cDados.getParametrosSistema("urlTelaInclusaoProjeto", "mostrarValoresReceita", "mostrarBoletinsUnidade", "MostraFisicoLaranja",
                                                              "MostraUnidadeAtendimento", "MostraGerenteProjeto", "MostraCategoria", "MostraStatus", "MostraCorGeral",
                                                              "MostraCorRisco", "MostraCorFisico", "MostraCorDespesa", "MostraCorEscopo", "MostraCorReceita",
                                                              "MostraCorTrabalho", "MostraCorMeta", "MostraCor_IAM", "MostraCorConvenio", "MostraCR",
                                                              "cig_Custo", "cig_Escopo", "cig_Fisico", "cig_Trabalho", "cig_Receita", "cig_Risco", "cig_Meta", "cig_IAM", "cig_convenio",
                                                              "MostraUltimaAtualizacao", "labelDespesa", "codigoCarteiraListaProcessos", "tituloTelaListaProcessos");
            displayUnidadeAtendimento = "";
            displayGerenteProjeto = "";
            displayCategoria = "";
            displayStatus = "";
            displayCR = "";
            codigoCarteiraTela = int.Parse(cDados.getInfoSistema("CodigoCarteira").ToString());
            lblCategorias.Text = Resources.traducao.resumoProjetos_categoria_;
            if (dsParametros.Tables[0].Rows[0]["codigoCarteiraListaProcessos"].ToString().Trim() != "")
                codigoCarteiraListaProcessos = int.Parse(dsParametros.Tables[0].Rows[0]["codigoCarteiraListaProcessos"].ToString().Trim());


            if (cDados.DataSetOk(dsParametros) && cDados.DataTableOk(dsParametros.Tables[0]))
            {
                mostrarBoletins = dsParametros.Tables[0].Rows[0]["mostrarBoletinsUnidade"] + "" != "N";

                if (dsParametros.Tables[0].Rows[0]["MostraFisicoLaranja"].ToString().Trim() == "N")
                {
                    tdDisplayLaranja.Style.Add(HtmlTextWriterStyle.Display, "none");
                    tdDisplayLaranja1.Style.Add(HtmlTextWriterStyle.Display, "none");
                }

                if (dsParametros.Tables[0].Rows[0]["MostraUnidadeAtendimento"].ToString().Trim() == "N")
                {
                    tdFiltroUnidadeAtendimento.Style.Add(HtmlTextWriterStyle.Display, "none");
                    tdFiltroUnidadeAtendimento1.Style.Add(HtmlTextWriterStyle.Display, "none");
                }

                if (dsParametros.Tables[0].Rows[0]["MostraGerenteProjeto"].ToString().Trim() == "N")
                {
                    td_FiltroGerente.Style.Add(HtmlTextWriterStyle.Display, "none");
                    td_FiltroGerente1.Style.Add(HtmlTextWriterStyle.Display, "none");
                }

                if (dsParametros.Tables[0].Rows[0]["MostraCategoria"].ToString().Trim() == "N")
                {
                    td_FiltroTipo.Style.Add(HtmlTextWriterStyle.Display, "none");
                    td_FiltroTipo1.Style.Add(HtmlTextWriterStyle.Display, "none");
                }

                if (dsParametros.Tables[0].Rows[0]["MostraStatus"].ToString().Trim() == "N")
                {
                    td_filtroStatus.Style.Add(HtmlTextWriterStyle.Display, "none");
                    td_filtroStatus1.Style.Add(HtmlTextWriterStyle.Display, "none");
                }

                // usando mais rigor ( != "S" ) para a coluna CR, já que é uma coluna 'não usual'
                if (dsParametros.Tables[0].Rows[0]["MostraCR"].ToString().Trim() != "S")
                {
                    td_filtroCR.Style.Add(HtmlTextWriterStyle.Display, "none");
                    td_filtroCR1.Style.Add(HtmlTextWriterStyle.Display, "none");
                }

            }


            bPodeIncluirProjetos = cDados.VerificaAcessoEmAlgumObjeto(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "UN", "UN_IncPrj");
            cDados.setaDefinicoesBotoesInserirExportarTreeList(menu, bPodeIncluirProjetos, "window.top.showModal('./administracao/CadastroProjetos.aspx?PopUP=S', traducao.resumoProjetos_inclus_o_de_projeto, null, null, atualizaGrid, null); ", true, true, true, "LstProjEn", "Projetos da Instituição", this, tlProjetos);

            if (!IsPostBack)
                getLabelsBancoDados();

            //if (!IsPostBack || tlProjetos.IsCallback)
            getProjetos();

            cDados.configuraPainelBotoesTREELIST(tbBotoesEdicao);

            //cDados.aplicaEstiloVisual(this);
        }

        private void getLabelsBancoDados()
        {
            //tlProjetos.Columns["NomeUnidadeAtendimento"].Visible = displayUnidadeAtendimento == "" ? tlProjetos.Columns["NomeUnidadeAtendimento"].Visible : false;
            tlProjetos.Columns["NomeUnidadeAtendimento"].Caption = cDados.defineLabelUnidadeAtendimento().Replace(":", " ").TrimEnd();

            DataSet ds = cDados.getParametrosSistema("labelGerente");
            if (cDados.DataSetOk(ds))
            {
                string labelGerente = ds.Tables[0].Rows[0]["labelGerente"] + "" != "" ? ds.Tables[0].Rows[0]["labelGerente"] + "" : "Gerente";

                lblGerentes.Text = labelGerente + ":";
                tlProjetos.Columns["GerenteProjeto"].Caption = labelGerente;
            }

            tlProjetos.Columns["GerenteProjeto"].Visible = displayGerenteProjeto == "" ? tlProjetos.Columns["GerenteProjeto"].Visible : false;
            //tlProjetos.Columns["TipoProjeto"].Visible = displayCategoria == "" ? tlProjetos.Columns["TipoProjeto"].Visible : false;
            tlProjetos.Columns["Status"].Visible = displayStatus == "" ? tlProjetos.Columns["Status"].Visible : false;
            tlProjetos.Columns["CR"].Visible = dsParametros.Tables[0].Rows[0]["MostraCR"].ToString().Trim() == "S" ? tlProjetos.Columns["CR"].Visible : false;

            tlProjetos.Columns["CorGeral"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorGeral"].ToString().Trim() == "S" ? tlProjetos.Columns["CorGeral"].Visible : false;
            tlProjetos.Columns["CorRisco"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorRisco"].ToString().Trim() == "S" ? tlProjetos.Columns["CorRisco"].Visible : false;
            tlProjetos.Columns["CorFisico"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorFisico"].ToString().Trim() == "S" ? tlProjetos.Columns["CorFisico"].Visible : false;
            tlProjetos.Columns["CorFinanceiro"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorDespesa"].ToString().Trim() == "S" ? tlProjetos.Columns["CorFinanceiro"].Visible : false;
            tlProjetos.Columns["CorEscopo"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorEscopo"].ToString().Trim() == "S" ? tlProjetos.Columns["CorEscopo"].Visible : false;
            tlProjetos.Columns["CorReceita"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorReceita"].ToString().Trim() == "S" && dsParametros.Tables[0].Rows[0]["mostrarValoresReceita"].ToString().Trim() != "N" ? tlProjetos.Columns["CorReceita"].Visible : false;
            tlProjetos.Columns["corTrabalho"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorTrabalho"].ToString().Trim() == "S" ? tlProjetos.Columns["corTrabalho"].Visible : false;
            tlProjetos.Columns["corMeta"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorMeta"].ToString().Trim() == "S" ? tlProjetos.Columns["corMeta"].Visible : false;
            tlProjetos.Columns["corIAM"].Visible = dsParametros.Tables[0].Rows[0]["MostraCor_IAM"].ToString().Trim() == "S" ? tlProjetos.Columns["corIAM"].Visible : false;
            tlProjetos.Columns["corConvenio"].Visible = dsParametros.Tables[0].Rows[0]["MostraCorConvenio"].ToString().Trim() == "S" ? tlProjetos.Columns["corConvenio"].Visible : false;
            tlProjetos.Columns["UltimaAtualizacao"].Visible = dsParametros.Tables[0].Rows[0]["MostraUltimaAtualizacao"].ToString().Trim() == "S" ? tlProjetos.Columns["UltimaAtualizacao"].Visible : false;

            tlProjetos.Columns["CorGeral"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorGeral"].ToString().Trim() == "S";
            tlProjetos.Columns["CorRisco"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorRisco"].ToString().Trim() == "S";
            tlProjetos.Columns["CorFisico"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorFisico"].ToString().Trim() == "S";
            tlProjetos.Columns["CorFinanceiro"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorDespesa"].ToString().Trim() == "S";
            tlProjetos.Columns["CorEscopo"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorEscopo"].ToString().Trim() == "S";
            tlProjetos.Columns["CorReceita"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorReceita"].ToString().Trim() == "S" && dsParametros.Tables[0].Rows[0]["mostrarValoresReceita"].ToString().Trim() != "N";
            tlProjetos.Columns["corTrabalho"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorTrabalho"].ToString().Trim() == "S";
            tlProjetos.Columns["corMeta"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorMeta"].ToString().Trim() == "S";
            tlProjetos.Columns["corIAM"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCor_IAM"].ToString().Trim() == "S";
            tlProjetos.Columns["corConvenio"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraCorConvenio"].ToString().Trim() == "S";
            tlProjetos.Columns["UltimaAtualizacao"].ShowInCustomizationForm = dsParametros.Tables[0].Rows[0]["MostraUltimaAtualizacao"].ToString().Trim() == "S";

            tlProjetos.Columns["CorRisco"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_Risco"].ToString().Trim() == "0";
            tlProjetos.Columns["CorFisico"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_Fisico"].ToString().Trim() == "0";
            tlProjetos.Columns["CorFinanceiro"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_Custo"].ToString().Trim() == "0";
            tlProjetos.Columns["CorEscopo"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_Escopo"].ToString().Trim() == "0";
            tlProjetos.Columns["CorReceita"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_Receita"].ToString().Trim() == "0";
            tlProjetos.Columns["corTrabalho"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_Trabalho"].ToString().Trim() == "0";
            tlProjetos.Columns["corMeta"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_Meta"].ToString().Trim() == "0";
            tlProjetos.Columns["corIAM"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_IAM"].ToString().Trim() == "0";
            tlProjetos.Columns["corConvenio"].HeaderStyle.Font.Strikeout = dsParametros.Tables[0].Rows[0]["cig_convenio"].ToString().Trim() == "0";
            tlProjetos.Columns["CorFinanceiro"].Caption = dsParametros.Tables[0].Rows[0]["labelDespesa"].ToString();


            tlProjetos.Columns["CorGeral"].Width = new Unit("100px");
            tlProjetos.Columns["CorFisico"].Width = new Unit("100px");
            tlProjetos.Columns["CorReceita"].Width = new Unit("100px");

        }

        private void verificaFiltrosParametro()
        {
            bool possuiParametro = false;

            if (Request.QueryString["Verde"] != null)
            {
                checkVerde.Checked = (Request.QueryString["Verde"].ToString() == "S");
                possuiParametro = true;
            }

            if (Request.QueryString["Amarelo"] != null)
            {
                checkAmarelo.Checked = (Request.QueryString["Amarelo"].ToString() == "S");
                possuiParametro = true;
            }

            if (Request.QueryString["Vermelho"] != null)
            {
                checkVermelho.Checked = (Request.QueryString["Vermelho"].ToString() == "S");
                possuiParametro = true;
            }

            if (Request.QueryString["Branco"] != null)
            {
                checkBranco.Checked = (Request.QueryString["Branco"].ToString() == "S");
                possuiParametro = true;
            }

            if (Request.QueryString["Laranja"] != null)
            {
                checkLaranja.Checked = (Request.QueryString["Laranja"].ToString() == "S");
                possuiParametro = true;
            }

            if (Request.QueryString["CUN"] != null)
            {
                ddlUnidades.Value = int.Parse(Request.QueryString["CUN"].ToString());
                possuiParametro = true;
            }

            if (Request.QueryString["CUNAT"] != null)
            {
                ddlUnidadeAtendimento.Value = int.Parse(Request.QueryString["CUNAT"].ToString());
                possuiParametro = true;
            }

            if (Request.QueryString["CodigoGerente"] != null)
            {
                ddlGerentes.Value = int.Parse(Request.QueryString["CodigoGerente"].ToString());
                possuiParametro = true;
            }

            if (Request.QueryString["Status"] != null)
            {
                ddlStatus.Value = Request.QueryString["Status"].ToString();
                possuiParametro = true;
            }

            if (Request.QueryString["Programas"] != null)
                possuiParametro = true;

            if (Request.QueryString["Programas"] + "" != "N")
            {
                ckProgramas.Checked = true;
            }

            if (!possuiParametro)
            {
                setFiltrosLayout();
            }
        }

        private void getProjetos()
        {
            string iniciaisTipoProjetoSelecao = "";

            if (Request.QueryString.Count > 0)
            {
                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (key.Contains("IniTipoPrj"))
                    {
                        iniciaisTipoProjetoSelecao += ",'" + Request.QueryString[key].ToString() + "'";
                        if (Request.QueryString[key].ToString() == "PRC")
                        {
                            if (dsParametros.Tables[0].Rows[0]["codigoCarteiraListaProcessos"].ToString().Trim() != "")
                                codigoCarteiraListaProcessos = int.Parse(dsParametros.Tables[0].Rows[0]["codigoCarteiraListaProcessos"].ToString().Trim());
                        }
                    }
                }
            }

            if (iniciaisTipoProjetoSelecao == "")
                iniciaisTipoProjetoSelecao = "'PRJ','PRG','PRC','SPT', 'PTF'";
            else
                iniciaisTipoProjetoSelecao = iniciaisTipoProjetoSelecao.Substring(1);

            if (!iniciaisTipoProjetoSelecao.Contains("PRG"))
                ckProgramas.Visible = false;

            if (!iniciaisTipoProjetoSelecao.Contains("PRJ"))
            {
                script = @"window.top.lblCarteira.SetVisible(false);
                       window.top.ddlVisaoInicial.SetVisible(false);
                       window.top.imgHistoricoStatusReport.SetVisible(false);
                       //window.top.document.getElementById('ctl00_imgStatusReport').style.display = 'none';  ";

                //Master.scriptJs = script;

                menu.Items[0].ClientVisible = false;
            }

            //if (iniciaisTipoProjetoSelecao.Contains("PRC"))
            //{
            //    if (dsParametros.Tables[0].Rows[0]["tituloTelaListaProcessos"].ToString().Trim() != "")
            //        lblTituloTela.Text = dsParametros.Tables[0].Rows[0]["tituloTelaListaProcessos"].ToString().Trim();
            //}

            string where = getFiltros(iniciaisTipoProjetoSelecao);

            dsProjetos = cDados.getListaProjetos(codigoEntidadeUsuarioResponsavel, int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString()), codigoCarteiraTela, codigoCarteiraListaProcessos, where);

            carregaTreeListProjetos(dsProjetos);
        }

        private string getFiltros(string iniciaisTipoProjetoSelecao)
        {
            //¥
            string where = "", whereCor = "";

            if (checkVerde.Checked)
                whereCor += " 'Verde',";

            if (checkAmarelo.Checked)
                whereCor += " 'Amarelo',";

            if (checkVermelho.Checked)
                whereCor += " 'Vermelho',";

            if (checkBranco.Checked)
                whereCor += " 'Branco',";

            if (checkLaranja.Checked)
                whereCor += " 'Laranja',";

            if (whereCor != "")
                where += string.Format(" AND rp.CorGeral IN(" + whereCor + "'Azul')", cDados.getDbName(), cDados.getDbOwner());
            else
                where = string.Format(" AND rp.CorGeral IN('Azul')", cDados.getDbName(), cDados.getDbOwner());

            if (displayGerenteProjeto == "" && ddlGerentes.Value != null && ddlGerentes.Value.ToString() != "-1")
                where += " AND p.CodigoGerenteProjeto = " + ddlGerentes.Value;

            if (ddlUnidades.Items.FindByValue(int.Parse(ddlUnidades.Value.ToString())) == null)
                ddlUnidades.SelectedIndex = 0;

            if (ddlUnidades.Value.ToString() != "-1")
                where += " AND p.CodigoUnidadeNegocio = " + ddlUnidades.Value;

            if (displayUnidadeAtendimento == "" && ddlUnidadeAtendimento.Items.FindByValue(int.Parse(ddlUnidadeAtendimento.Value.ToString())) == null)
                ddlUnidadeAtendimento.SelectedIndex = 0;

            if (displayUnidadeAtendimento == "" && ddlUnidadeAtendimento.Value.ToString() != "-1")
                where += " AND (p.CodigoUnidadeAtendimento = " + ddlUnidadeAtendimento.Value.ToString()
                    + " OR  dbo.f_GetUnidadeSuperior(p.[CodigoUnidadeAtendimento], " + ddlUnidadeAtendimento.Value + ") IS NOT NULL)";

            if (displayCategoria == "" && ddlCategoria.Value.ToString() != "-1")
                where += " AND p.CodigoCategoria = " + ddlCategoria.Value;

            if (displayStatus == "" && ddlStatus.Value.ToString() != "-1")
                where += " AND p.CodigoStatusProjeto = " + ddlStatus.Value;

            if (displayCR == "" && ddlCR.Value.ToString() != "-1")
                where += " AND dbo.f_GetCodigoCrPrincipal(p.CodigoProjeto) = " + ddlCR.Value;

            if (txtDescricao.Text != "")
                where += " AND p.NomeProjeto LIKE '%" + txtDescricao.Text.Replace("'", "''") + "%'";

            if (!ckProgramas.Checked)
                where += " AND p.IndicaPrograma = 'N'";

            DateTime dataReferencia;
            if (DateTime.TryParse(Request.QueryString["dtRef"], out dataReferencia))
                where += string.Format(" AND UltimaAtualizacao < CONVERT ( datetime , '{0:d}', 103)", dataReferencia);

            where += " AND p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto IN (" + iniciaisTipoProjetoSelecao + "))";

            return where;
        }

        private string getFiltrosLayout()
        {
            //¥
            string where = "";

            if (!checkVerde.Checked)
                where += ";;checkVerde;;¥";

            if (!checkAmarelo.Checked)
                where += ";;checkAmarelo;;¥";

            if (!checkVermelho.Checked)
                where += ";;checkVermelho;;¥";

            if (!checkBranco.Checked)
                where += ";;checkBranco;;¥";

            if (!checkLaranja.Checked)
                where += ";;checkLaranja;;¥";

            if (ddlGerentes.Value.ToString() != "-1")
            {
                where += ";;ddlGerentes;;" + ddlGerentes.Value + "¥";
                where += ";;txtGerentes;;" + ddlGerentes.Text + "¥";
            }

            if (ddlUnidades.Value.ToString() != "-1")
                where += ";;ddlUnidades;;" + ddlUnidades.Value + "¥";

            if (displayUnidadeAtendimento == "" && ddlUnidadeAtendimento.Value.ToString() != "-1")
                where += ";;ddlUnidadeAtendimento;;" + ddlUnidadeAtendimento.Value + "¥";

            if (displayCategoria == "" && ddlCategoria.Value.ToString() != "-1")
                where += ";;ddlCategoria;;" + ddlCategoria.Value + "¥";

            if (displayStatus == "" && ddlStatus.Value.ToString() != "-1")
                where += ";;ddlStatus;;" + ddlStatus.Value + "¥";

            if (displayCR == "" && ddlCR.Value.ToString() != "-1")
                where += ";;ddlCR;;" + ddlCR.Value + "¥";

            if (txtDescricao.Text != "")
                where += ";;txtDescricao;;" + txtDescricao.Text + "¥";

            if (!ckProgramas.Checked)
                where += ";;ckProgramas;;N¥";

            return where;
        }

        private void setFiltrosLayout()
        {
            ddlGerentes.Value = -1;
            ddlUnidades.Value = -1;
            ddlUnidadeAtendimento.Value = -1;
            ddlCategoria.Value = "-1";
            ddlStatus.Value = "-1";
            ddlCR.Value = "-1";
            txtDescricao.Text = "";
            ckProgramas.Checked = true;
            checkVerde.Checked = true;
            checkAmarelo.Checked = true;
            checkVermelho.Checked = true;
            checkBranco.Checked = true;
            checkLaranja.Checked = true;

            string comandoSQL = string.Format(@"SELECT FiltroAplicado 
                                                  FROM {0}.{1}.ListaUsuario 
                                                 WHERE CodigoUsuario = {3}
                                                   AND CodigoLista = (SELECT CodigoLista FROM {0}.{1}.Lista WHERE CodigoEntidade = {2} AND IniciaisListaControladaSistema = 'LstProjEn')"
                , cDados.getDbName(), cDados.getDbOwner(), codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel);

            DataSet ds = cDados.getDataSet(comandoSQL);

            if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
            {
                if (ds.Tables[0].Rows[0]["FiltroAplicado"].ToString().Contains(";;checkVerde;;"))
                    checkVerde.Checked = false;
                if (ds.Tables[0].Rows[0]["FiltroAplicado"].ToString().Contains(";;checkAmarelo;;"))
                    checkAmarelo.Checked = false;
                if (ds.Tables[0].Rows[0]["FiltroAplicado"].ToString().Contains(";;checkVermelho;;"))
                    checkVermelho.Checked = false;
                if (ds.Tables[0].Rows[0]["FiltroAplicado"].ToString().Contains(";;checkBranco;;"))
                    checkBranco.Checked = false;
                if (ds.Tables[0].Rows[0]["FiltroAplicado"].ToString().Contains(";;checkLaranja;;"))
                    checkLaranja.Checked = false;

                foreach (string valorCampo in ds.Tables[0].Rows[0]["FiltroAplicado"].ToString().Split('¥'))
                {
                    if (valorCampo.Contains(";;ddlGerentes;;"))
                    {
                        ddlGerentes.Value = valorCampo.Replace(";;ddlGerentes;;", "");
                        if (ddlGerentes.Value != null && ddlGerentes.Value.ToString() != "")
                        {
                            int codigoGerente = -1;
                            Int32.TryParse(ddlGerentes.Value.ToString(), out codigoGerente);

                            string comandoSQLUsuario = string.Format(@"SELECT NomeUsuario FROM Usuario WHERE CodigoUsuario = " + codigoGerente);

                            DataSet dsUsuario = cDados.getDataSet(comandoSQLUsuario);

                            if (cDados.DataSetOk(dsUsuario) && cDados.DataTableOk(dsUsuario.Tables[0]))
                            {
                                ddlGerentes.Text = dsUsuario.Tables[0].Rows[0]["NomeUsuario"].ToString();
                                ddlGerentes.Value = codigoGerente;
                            }
                        }
                    }
                    else if (valorCampo.Contains(";;ddlUnidades;;"))
                        ddlUnidades.Value = int.Parse(valorCampo.Replace(";;ddlUnidades;;", ""));
                    else if (valorCampo.Contains(";;ddlUnidadeAtendimento;;"))
                        ddlUnidadeAtendimento.Value = valorCampo.Replace(";;ddlUnidadeAtendimento;;", "");
                    else if (valorCampo.Contains(";;ddlCategoria;;"))
                        ddlCategoria.Value = valorCampo.Replace(";;ddlCategoria;;", "");
                    else if (valorCampo.Contains(";;ddlStatus;;"))
                        ddlStatus.Value = valorCampo.Replace(";;ddlStatus;;", "");
                    else if (valorCampo.Contains(";;ddlCR;;"))
                        ddlCR.Value = valorCampo.Replace(";;ddlCR;;", "");
                    else if (valorCampo.Contains(";;txtDescricao;;"))
                        txtDescricao.Text = valorCampo.Replace(";;txtDescricao;;", "");
                    else if (valorCampo.Contains(";;ckProgramas;;"))
                        ckProgramas.Checked = false;
                }
            }
        }

        public void carregaTreeListProjetos(DataSet ds)
        {
            tlProjetos.DataSource = ds.Tables[0];
            tlProjetos.DataBind();
            tlProjetos.ExpandToLevel(6);
        }

        #region Combos Filtros

        private void carregaCombo(DataSet dsCombo, string textField, string valueField, ASPxComboBox ddlCombo, string customItem, string valueCustomItem)
        {
            if (cDados.DataSetOk(dsCombo))
            {
                ddlCombo.DataSource = dsCombo;
                ddlCombo.TextField = textField;
                ddlCombo.ValueField = valueField;
            }

            if (textField == "NomeUsuario")
            {
                if (customItem != "")
                {
                    DataRow dr = dsCombo.Tables[0].NewRow();
                    dr["CodigoUsuario"] = int.Parse(valueCustomItem);
                    dr["NomeUsuario"] = customItem;
                    dr["EMail"] = customItem;
                    dsCombo.Tables[0].Rows.InsertAt(dr, 0);
                }
                ddlCombo.Columns[0].FieldName = "NomeUsuario";
                ddlCombo.Columns[1].FieldName = "EMail";
                ddlCombo.TextFormatString = "{0}";
            }

            if (textField == "DescricaoCategoria")
            {
                if (customItem != "")
                {
                    DataRow dr = dsCombo.Tables[0].NewRow();
                    dr["CodigoCategoria"] = int.Parse(valueCustomItem);
                    dr["DescricaoCategoria"] = customItem;
                    dr["SiglaCategoria"] = customItem;
                    dsCombo.Tables[0].Rows.InsertAt(dr, 0);
                }
                ddlCombo.Columns[0].FieldName = "DescricaoCategoria";
                ddlCombo.Columns[1].FieldName = "SiglaCategoria";
                ddlCombo.TextFormatString = "{0}";
            }

            if (textField == "SiglaUnidadeNegocio")
            {

                if (customItem != "")
                {
                    DataRow dr = dsCombo.Tables[0].NewRow();
                    dr["CodigoUnidadeNegocio"] = int.Parse(valueCustomItem);
                    dr["SiglaUnidadeNegocio"] = customItem;
                    dr["NomeUnidadeNegocio"] = customItem;
                    dsCombo.Tables[0].Rows.InsertAt(dr, 0);
                }
                ddlCombo.Columns[0].FieldName = "SiglaUnidadeNegocio";
                ddlCombo.Columns[1].FieldName = "NomeUnidadeNegocio";
                ddlCombo.TextFormatString = "{0}";
            }

            if (textField == "DescricaoCR")
            {
                if (customItem != "")
                {
                    DataRow dr = dsCombo.Tables[0].NewRow();
                    dr["CodigoCR"] = int.Parse(valueCustomItem);
                    dr["DescricaoCR"] = customItem;
                    dsCombo.Tables[0].Rows.InsertAt(dr, 0);
                }
            }
            ddlCombo.DataBind();

            if ((!IsPostBack && !IsCallback) || ddlCombo.SelectedIndex == -1)
            {
                ddlCombo.SelectedIndex = 0;
            }
        }

        protected void ddlGerentes_ItemRequestedByValue(object source, ListEditItemRequestedByValueEventArgs e)
        {
            DataSet dsCombos;

            if (e.Value != null)
            {
                long value = 0;
                if (!Int64.TryParse(e.Value.ToString(), out value))
                    return;
                ASPxComboBox comboBox = (ASPxComboBox)source;

                string where = " AND CodigoUsuario = " + value;

                dsCombos = cDados.getGerentesProjetosEntidade(codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, int.Parse(cDados.getInfoSistema("CodigoCarteira").ToString()), where);
                carregaCombo(dsCombos, "NomeUsuario", "CodigoUsuario", comboBox, "", "-1");
            }

        }

        protected void ddlGerentes_ItemsRequestedByFilterCondition(object source, ListEditItemsRequestedByFilterConditionEventArgs e)
        {
            DataSet dsCombos;

            ASPxComboBox comboBox = (ASPxComboBox)source;

            if (displayGerenteProjeto == "")
            {
                string filtro = "";

                filtro = e.Filter.ToString();

                string where = "";

                if (filtro.Trim() != "")
                {
                    where = " AND ( u.NomeUsuario COLLATE SQL_LATIN1_GENERAL_CP1_CI_AI LIKE '%" + filtro.Replace("'", "''") + "%' OR u.EMail COLLATE SQL_LATIN1_GENERAL_CP1_CI_AI LIKE '%" + filtro.Replace("'", "''") + "%' )";
                }

                dsCombos = cDados.getGerentesProjetosEntidade(codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, int.Parse(cDados.getInfoSistema("CodigoCarteira").ToString()), where);
                carregaCombo(dsCombos, "NomeUsuario", "CodigoUsuario", comboBox, "", "-1");
            }
        }

        private void inicializaCombosFiltros()
        {
            DataSet dsCombos;

            if (displayGerenteProjeto == "")
            {
                //Carrega Combo de Gerentes
                dsCombos = cDados.getGerentesProjetosEntidade(codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, int.Parse(cDados.getInfoSistema("CodigoCarteira").ToString()), "");
                carregaCombo(dsCombos, "NomeUsuario", "CodigoUsuario", ddlGerentes, Resources.traducao.todos, "-1");
            }

            //Carrega Combo de Unidades
            dsCombos = cDados.getUnidadesNegocioListaProjetos(codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, int.Parse(cDados.getInfoSistema("CodigoCarteira").ToString()));
            carregaCombo(dsCombos, "SiglaUnidadeNegocio", "CodigoUnidadeNegocio", ddlUnidades, Resources.traducao.todos, "-1");

            if (displayUnidadeAtendimento == "")
            {
                //Carrega Combo de Unidades de atendimento
                dsCombos = cDados.getUnidade(string.Format(@" AND DataExclusao IS NULL AND IndicaUnidadeNegocioAtiva = 'S' "));
                carregaCombo(dsCombos, "SiglaUnidadeNegocio", "CodigoUnidadeNegocio", ddlUnidadeAtendimento, Resources.traducao.todas, "-1");
            }

            if (displayCategoria == "")
            {
                //Carrega Combo de Categorias
                dsCombos = cDados.getCategoriasEntidade(codigoEntidadeUsuarioResponsavel, "");
                carregaCombo(dsCombos, "DescricaoCategoria", "CodigoCategoria", ddlCategoria, Resources.traducao.todas, "-1");
            }

            if (displayStatus == "")
            {
                //Carrega Combo de Status
                dsCombos = cDados.getStatus(" AND IndicaAcompanhamentoExecucao = 'S' AND TipoStatus = 'PRJ'");
                carregaCombo(dsCombos, "DescricaoStatus", "CodigoStatus", ddlStatus, Resources.traducao.todos, "-1");


                ListEditItem lei = new ListEditItem(Resources.traducao.todos, "-1");
                ddlStatus.Items.Insert(0, lei);

                if (!IsPostBack && ddlStatus.Items.FindByValue("3") != null)
                    ddlStatus.Value = "3";
            }

            if (displayCR == "")
            {
                //Carrega Combo de CR
                dsCombos = getListaCR();
                carregaCombo(dsCombos, "DescricaoCR", "CodigoCR", ddlCR, Resources.traducao.todos, "-1");


                //ListEditItem lei = new ListEditItem(Resources.traducao.todos, "-1");
                //ddlCR.Items.Insert(0, lei);
            }
        }

        #endregion

        protected void tlProjetos_CustomCallback(object sender, DevExpress.Web.ASPxTreeList.TreeListCustomCallbackEventArgs e)
        {
            getProjetos();
            cDados.setInfoSistema("CodigoStatus", ddlStatus.Items.Count > 0 ? ddlStatus.Value.ToString() : "-1");
        }

        protected void tlProjetos_HtmlDataCellPrepared(object sender, DevExpress.Web.ASPxTreeList.TreeListHtmlDataCellEventArgs e)
        {
            //if (e.Column.Index == 0)
            //    e.Cell.Style.Add("width", "400px");
        }

        public string getDescricaoObjetosLista()
        {
            StringBuilder retornoHTML = new StringBuilder();

            string indicaProjeto = Eval("IndicaProjeto").ToString();
            string indicaPrograma = Eval("IndicaPrograma").ToString();
            string codigoProjeto = Eval("CodigoProjeto").ToString();
            string descricao = Eval("Descricao").ToString();
            string permissao = Eval("Permissao") + "";
            string codigoUnidade = Eval("CodigoUnidade").ToString();
            string codigoEntidadeParam = Eval("CodigoEntidade").ToString();
            string codigoStatus = Eval("CodigoStatusProjeto").ToString();
            string IniciaisTipoProjeto = Eval("IniciaisTipoProjeto").ToString();
            string IndicaProjetoAgil = Eval("IndicaProjetoAgil").ToString();
            string IniciaisTipoControlado = Eval("IniciaisTipoControladoSistema").ToString();

            if (indicaProjeto == "S")
            {
                retornoHTML.AppendLine("<table>");
                retornoHTML.AppendLine("<tr>");
                retornoHTML.AppendLine("<td>");
                string styleCor = "";

                //if (codigoStatus == "5" && displaySuspensos == "S")
                //    styleCor = "style='Color:#FF79BC'";
                //else if (codigoStatus == "4" && displayCancelados == "S")
                //    styleCor = "style='Color:#977DFB'";
                //else if (codigoStatus == "6" && displayEncerrados == "S")
                //    styleCor = "style='Color:#0000CC'";


                if (indicaPrograma == "S")
                {
                    if (IniciaisTipoProjeto == "PTF")
                    {
                        retornoHTML.AppendLine("<img border='0' src='../imagens/menu-portfolio.png' style='width: 21px; height: 18px;' title='" + Resources.traducao.resumoProjetos_portf_lios + "'/>");
                    }
                    else
                    {
                        retornoHTML.AppendLine("<img border='0' src='../imagens/programa.PNG' style='width: 21px; height: 18px;' title='" + Resources.traducao.resumoProjetos_programa + "'/>");
                    }
                }
                else if (IniciaisTipoProjeto == "PRC")
                {
                    retornoHTML.AppendLine("<img border='0' src='../imagens/processo.PNG' style='width: 21px; height: 18px;' title='" + Resources.traducao.resumoProjetos_processo + "'/>");
                }
                else if (IniciaisTipoProjeto == "SPT")
                {
                    retornoHTML.AppendLine("<img border='0' src='../imagens/sprint.PNG' style='width: 21px; height: 18px;' title='" + Resources.traducao.resumoProjetos_sprint + "'/>");
                }

                else if (IndicaProjetoAgil == "S")
                {
                    retornoHTML.AppendLine("<img border='0' src='../imagens/agile.PNG' style='width: 21px; height: 18px;' title='" + Resources.traducao.resumoProjetos_projeto__gil + "'/>");
                }
                else
                {
                    if (IniciaisTipoControlado == "DEM_GDH")
                    {
                        retornoHTML.AppendLine("<img border='0' src='../imagens/Logo-GDH.PNG' title='Demanda GDH'/>");
                    }
                    else
                    {
                        retornoHTML.AppendLine("<img border='0' src='../imagens/projeto.PNG' title='" + Resources.traducao.resumoProjetos_projeto + "'/>");
                    }
                }

                //Retirada a verificação de permissão (PBH). A linha abaixo foi colocada e as outras foram comentadas.
                retornoHTML.AppendLine("</td><td>&nbsp;<a  " + styleCor + " class='LinkGrid' href='./DadosProjeto/indexResumoProjeto.aspx?IDProjeto=" + codigoProjeto + "'>" + descricao + "</a>");

                //if (permissao != "" && bool.Parse(permissao) == true)
                //{
                //    retornoHTML += "</td><td>&nbsp;<a  " + styleCor + " class='LinkGrid' href='./DadosProjeto/indexResumoProjeto.aspx?IDProjeto=" + codigoProjeto + "&NomeProjeto=" + descricao + "'>" + descricao + "</a>";
                //}
                //else
                //{
                //    retornoHTML += "</td><td " + styleCor + ">&nbsp;" + descricao;
                //}

                retornoHTML.AppendLine("</td></tr></table>");
            }
            else
            {
                retornoHTML.AppendLine("<table><tr><td>" + descricao + "</td><td title='" + Resources.traducao.resumoProjetos_cadastrar_editar_reuni_es_da + " " + hfGeral.Get("definicaoUnidade").ToString() + "'>");

                if (permissao != "" && bool.Parse(permissao) == true)
                {
                    retornoHTML.AppendLine("<img alt='" + Resources.traducao.resumoProjetos_cadastrar_editar_reuni_es_da + " " + hfGeral.Get("definicaoUnidade").ToString() + "' style='cursor:pointer' onclick=abreReuniao(" + codigoUnidade + ") src='../imagens/reuniao.png'/>");
                }
                else
                {
                    retornoHTML.AppendLine("&nbsp");
                }

                retornoHTML.AppendLine("</td>");

                if (codigoEntidadeParam == codigoEntidadeUsuarioResponsavel.ToString() && mostrarBoletins)
                {
                    retornoHTML.AppendLine("<td title='" + Resources.traducao.resumoProjetos_boletins_da + " " + hfGeral.Get("definicaoUnidade").ToString() + "'><img alt='" + Resources.traducao.resumoProjetos_boletins_da + " " + hfGeral.Get("definicaoUnidade").ToString() + "' style='cursor:pointer' onclick=abreBoletins(" + codigoUnidade + ",'" + descricao.Replace(" ", "&nbsp;") + "')  src='../imagens/boletins.png' /></td>");
                }

                retornoHTML.AppendLine("</tr></table>");
            }
            return retornoHTML.ToString();
        }

        public string getItensLegenda()
        {
            StringBuilder retornoHTML = new StringBuilder();

            retornoHTML.AppendLine("< li >< a style = 'color: #5e585c;' >< i class='fas fa-circle ic-legend ic-verde'></i>'");
            retornoHTML.AppendLine("<asp:Literal runat = 'server' Text='<%$ Resources:traducao, resumoProjetos_satisfat_rio %>' /></a></li>'");
            retornoHTML.AppendLine("<li><a style = 'color: #5e585c;' >< i class='fas fa-circle ic-legend ic-amarelo'></i>'");
            retornoHTML.AppendLine("<asp:Literal runat = 'server' Text='<%$ Resources:traducao, resumoProjetos_em_aten__o %>' /></a></li>'");
            retornoHTML.AppendLine("<li><a style = 'color: #5e585c;' >< i class='fas fa-circle ic-legend ic-vermelho'></i>'");
            retornoHTML.AppendLine("<asp:Literal runat = 'server' Text='<%$ Resources:traducao, resumoProjetos_cr_tico %>' /></a></li>'");
            retornoHTML.AppendLine("<li><a style = 'color: #5e585c;' >< i class='fas fa-circle ic-legend ic-cinza'></i>'");
            retornoHTML.AppendLine("<asp:Literal runat = 'server' Text='<%$ Resources:traducao, resumoProjetos_sem_informa__o %>' /></a></li>'");
            retornoHTML.AppendLine("<li><a style = 'color: #5e585c;' >< i class='fas fa-circle ic-legend ic-azul'></i>'");
            retornoHTML.AppendLine("<asp:Literal runat = 'server' Text='<%$ Resources:traducao, resumoProjetos_encerrados %>' /></a></li>'");
            foreach (var item in itensLegenda)
            {
                retornoHTML.AppendLine(item);
            }

            return retornoHTML.ToString();
        }

        private void AdicinaItemListaLegenda(string item)
        {
            if (!itensLegenda.Contains(item))
            {
                itensLegenda.Add(item);
            }
        }
        #region Eventos Menu Botões Inserção e Exportação

        protected void menu_ItemClick(object source, DevExpress.Web.MenuItemEventArgs e)
        {
            string parameter = e.Item.Text == "" ? "XLS" : e.Item.Text;

            System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

            listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            cDados = CdadosUtil.GetCdados(listaParametrosDados);

            string where = getFiltrosLayout();

            cDados.eventoClickMenuTreeList((source as ASPxMenu), parameter, ASPxTreeListExporter1, "LstProjEn", tlProjetos, where);

            setFiltrosLayout();
            getLabelsBancoDados();
            getProjetos();
        }

        protected void menu_Init(object sender, EventArgs e)
        {
            //System.Collections.Specialized.OrderedDictionary listaParametrosDados = new System.Collections.Specialized.OrderedDictionary();

            //listaParametrosDados["RemoteIPUsuario"] = Session["RemoteIPUsuario"] + "";
            //listaParametrosDados["NomeUsuario"] = Session["NomeUsuario"] + "";

            //cDados = CdadosUtil.GetCdados(listaParametrosDados);
            //try
            //{
            //    if (cDados.getInfoSistema("IDUsuarioLogado") == null)
            //        Response.Redirect("~/erros/erroInatividade.aspx");
            //}
            //catch
            //{
            //    Response.RedirectLocation = cDados.getPathSistema() + "erros/erroInatividade.aspx";
            //    Response.End();
            //}
            //codigoUsuarioResponsavel = int.Parse(cDados.getInfoSistema("IDUsuarioLogado").ToString());
            //codigoEntidadeUsuarioResponsavel = int.Parse(cDados.getInfoSistema("CodigoEntidade").ToString());


        }

        #endregion

        protected void ASPxTreeListExporter1_RenderBrick(object sender, DevExpress.Web.ASPxTreeList.ASPxTreeListExportRenderBrickEventArgs e)
        {


            if ((e.Column.FieldName == "CorGeral" || e.Column.FieldName == "CorRisco" || e.Column.FieldName == "CorFisico" ||
                e.Column.FieldName == "CorFinanceiro" || e.Column.FieldName == "CorEscopo" || e.Column.FieldName == "CorReceita" ||
                            e.Column.FieldName == "corTrabalho" || e.Column.FieldName == "corMeta" || e.Column.FieldName == "corIAM" || e.Column.FieldName == "corConvenio") &&
                (e.Text != "") &&
                (e.RowKind == DevExpress.Web.ASPxTreeList.TreeListRowKind.Data))
            {
                Font fontex = new Font("Wingdings", 18, FontStyle.Bold, GraphicsUnit.Point);
                e.BrickStyle.Font = fontex;

                if (e.Text.ToString().Contains("Vermelho"))
                {
                    e.BrickStyle.ForeColor = Color.Red;
                }
                if (e.Text.ToString().Contains("Amarelo"))
                {
                    e.BrickStyle.ForeColor = Color.Yellow;
                }
                if (e.Text.ToString().Contains("Verde"))
                {
                    e.BrickStyle.ForeColor = Color.Green;
                }
                if (e.Text.ToString().Contains("Branco"))
                {
                    e.BrickStyle.ForeColor = Color.WhiteSmoke;
                }
                if (e.Text.ToString().Contains("Azul"))
                {
                    e.BrickStyle.ForeColor = Color.Blue;
                }
                if (e.Text.ToString().Contains("Laranja"))
                {
                    e.BrickStyle.ForeColor = Color.Orange;
                }
                e.Text = "l";
                e.TextValue = "l";
            }
        }

        private DataSet getListaCR()
        {
            string comandoSQL = string.Format(@"SELECT * FROM {0}.{1}.f_GetOpcoesCRListaIniciativas({2}) ORDER BY [DescricaoCR];", cDados.getDbName(), cDados.getDbOwner(), codigoEntidadeUsuarioResponsavel);
            return cDados.getDataSet(comandoSQL);
        }

    }
}
