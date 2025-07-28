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
using CDIS;

namespace BriskPtf._Projetos
{
    public partial class _Projetos_cadastroDemandasComplexas : System.Web.UI.Page
    {
        dados cDados;

        private int codigoUsuarioResponsavel;
        private int codigoEntidadeUsuarioResponsavel;
        private int codigoDemanda = -1;
        private bool somenteLeituraGrid = false;
        private ASPxGridView gvToDoList;

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

            if (Request.QueryString["CP"] != null && Request.QueryString["CP"].ToString() != "")
                codigoDemanda = int.Parse(Request.QueryString["CP"].ToString());

            carregaComboUnidades();

            hfGeral.Set("TipoOperacao", "Editar");
            dsResponsavel.ConnectionString = cDados.classeDados.getStringConexao();
            //cDados.aplicaEstiloVisual(Page);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            HeaderOnTela();
            if (codigoDemanda != 0 || hfGeral.Contains("NovoCodigoDemanda"))
            {
                if (codigoDemanda == 0)
                    codigoDemanda = int.Parse(hfGeral.Get("NovoCodigoDemanda").ToString());

                if (!IsPostBack && !IsCallback)
                    montaCampos();
            }
            else
            {
                if (!IsPostBack)
                {
                    tcDemanda.TabPages[1].ClientVisible = false;
                    //tcDemanda.TabPages[2].ClientVisible = false;
                    hfGeral.Set("CodigoResponsavel", "-1");
                    hfGeral.Set("CodigoDemandante", "-1");
                }
            }

            if (((cDados.getInfoSistema("DesabilitarBotoes") != null && cDados.getInfoSistema("DesabilitarBotoes").ToString() == "S")) ||
                ((Request.QueryString["RO"] != null) && (Request.QueryString["RO"] == "S")))
            {
                somenteLeituraGrid = true;
            }


            int[] convidados = getParticipantesDemanda();

            Unit tamanho = new Unit(100, UnitType.Percentage);
            int codigoTipoAssociacao = cDados.getCodigoTipoAssociacao("PR");

            hfGeral.Set("codigoObjetoAssociado", codigoDemanda);

            PlanoDeAcao myPlanoDeAcao = null;
            myPlanoDeAcao = new PlanoDeAcao(cDados.classeDados, codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, codigoDemanda, codigoTipoAssociacao, codigoDemanda, tamanho, 250, somenteLeituraGrid, convidados.Length == 0 ? null : convidados, true, txtTitulo.Text);
            tcDemanda.TabPages.FindByName("tabTarefas").Controls.AddAt(0, myPlanoDeAcao.constroiInterfaceFormulario());
            gvToDoList = myPlanoDeAcao.gvToDoList;
            gvToDoList.Font.Name = "Verdana";
            gvToDoList.Font.Size = 8;
            gvToDoList.ClientInstanceName = "gvToDoList";
            gvToDoList.ClientSideEvents.BeginCallback = "function(s, e) { hfGeralToDoList.Set('NomeToDoList',  txtTitulo.GetText());}";


            if (!IsCallback)
                gvToDoList.DataBind();

            if (!IsPostBack)
            {
                if (((cDados.getInfoSistema("DesabilitarBotoes") != null && cDados.getInfoSistema("DesabilitarBotoes").ToString() == "S")) ||
                       ((Request.QueryString["RO"] != null) && (Request.QueryString["RO"] == "S")))
                {
                    btnSalvar.ClientVisible = false;
                    txtTitulo.ReadOnly = true;
                    txtDetalhes.ReadOnly = true;
                    txtInicio.ReadOnly = true;
                    // todo: incluir controle para os demais campos.
                    txtTermino.ReadOnly = true;
                    ddlPrioridade.ReadOnly = true;
                    ddlDemandante.ClientEnabled = false;
                    ddlResponsavel.ClientEnabled = false;
                    ddlUnidade.ReadOnly = true;
                }
            }

            string where = string.Format(@" DataExclusao IS NULL AND CodigoUsuario IN(SELECT uun.CodigoUsuario FROM {0}.{1}.UsuarioUnidadeNegocio AS uun 
								                                  WHERE Uun.CodigoUnidadeNegocio = {2} AND uun.[IndicaUsuarioAtivoUnidadeNegocio] = 'S')", cDados.getDbName(), cDados.getDbOwner(), codigoEntidadeUsuarioResponsavel.ToString());
            // conteúdo usado na tela para listar os usuários
            hfGeral.Set("ComandoWhere", where);

            tcDemanda.TabIndex = 0;
            populaCombos();
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/PropostaResumo.js""></script>"));
            this.TH(this.TS("PropostaResumo"));
        }

        #region VARIOS

        private void HeaderOnTela()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Header.Controls.Add(cDados.getLiteral(@"<link href=""../estilos/cdisEstilos.css"" rel=""stylesheet"" type=""text/css"" />"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/cadastroDemandaSimples.js""></script>"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/cadastroDemandasComplexas.js""></script>"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/_Strings.js""></script>"));

        }
        #endregion

        private void montaCampos()
        {
            DataSet dsDados = cDados.getDadosDemanda(codigoDemanda, "");

            if (cDados.DataSetOk(dsDados) && cDados.DataTableOk(dsDados.Tables[0]))
            {
                DataRow dr = dsDados.Tables[0].Rows[0];

                txtTitulo.Text = dr["Descricao"].ToString();
                txtDetalhes.Text = dr["DescricaoProposta"].ToString();
                txtInicio.Text = dr["InicioProposta"].ToString();
                txtTermino.Text = dr["TerminoProposta"].ToString();
                ddlPrioridade.Value = dr["Prioridade"].ToString();
                ddlDemandante.Text = dr["NomeDemandante"].ToString();
                ddlResponsavel.Text = dr["NomeGerente"].ToString();
                ddlUnidade.Value = dr["CodigoUnidade"].ToString();

                hfGeral.Set("CodigoResponsavel", dr["CodigoGerenteProjeto"].ToString());
                hfGeral.Set("CodigoDemandante", dr["CodigoDemandante"].ToString());
            }
        }

        private void carregaComboUnidades()
        {
            DataSet dsUnidades = cDados.getUnidadesNegocioEntidade(codigoEntidadeUsuarioResponsavel, "");

            if (cDados.DataSetOk(dsUnidades))
            {
                ddlUnidade.DataSource = dsUnidades;
                ddlUnidade.TextField = "NomeUnidadeNegocio";
                ddlUnidade.ValueField = "CodigoUnidadeNegocio";
                ddlUnidade.DataBind();

                if (!IsPostBack && ddlUnidade.Items.Count > 0)
                    ddlUnidade.SelectedIndex = 0;
            }
        }

        private int[] getParticipantesDemanda()
        {
            DataSet dsConvidados = cDados.getUsuarioDaEntidadeAtiva(codigoEntidadeUsuarioResponsavel.ToString(), "");

            int[] convidados = new int[dsConvidados.Tables[0].Rows.Count];

            if (cDados.DataSetOk(dsConvidados))
            {
                int i = 0;
                foreach (DataRow dr in dsConvidados.Tables[0].Rows)
                {
                    convidados[i] = int.Parse(dr["CodigoUsuario"].ToString());
                    i++;
                }
            }

            return convidados;
        }

        protected void callbackSalvar_Callback(object source, DevExpress.Web.CallbackEventArgs e)
        {
            int codigoDemandante, codigoResponsavel, codigoUnidade, novoCodigoDemanda = -1;
            string msgErro = "", msgToShow = "";

            codigoUnidade = ddlUnidade.SelectedIndex == -1 ? -1 : int.Parse(ddlUnidade.Value.ToString());

            if (int.TryParse(ddlDemandante.Value == null ? "-1" : ddlDemandante.Value.ToString(), out codigoDemandante)) { }
            if (int.TryParse(ddlResponsavel.Value == null ? "-1" : ddlResponsavel.Value.ToString(), out codigoResponsavel)) { }

            if (codigoDemanda == 0)
            {
                bool resultado = cDados.incluiDemandaComplexa(txtTitulo.Text, txtDetalhes.Text, txtInicio.Text, txtTermino.Text, char.Parse(ddlPrioridade.Value.ToString())
                                                           , codigoDemandante, codigoUnidade, codigoResponsavel
                                                           , codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "4", ref msgErro, ref novoCodigoDemanda);

                msgToShow = resultado ? "As informações do formulário foram salvas" : "Erro ao salvar. " + msgErro;

                callbackSalvar.JSProperties["cp_status"] = resultado ? "ok" : "erro";

                if (novoCodigoDemanda != -1)
                {
                    callbackSalvar.JSProperties["cp_NovoCodigoDemanda"] = novoCodigoDemanda;

                    // a linha abaixo é necessária caso esta tela esteja sendo usada em um fluxo;
                    cDados.setInfoSistema("CodigoProjeto", novoCodigoDemanda);
                }
            }
            else
            {
                //CodigoStatus = [Aguardando Análisis] = [14]
                bool resultado = cDados.atualizaDemandaSimples(codigoDemanda, txtTitulo.Text, txtDetalhes.Text, txtInicio.Text, txtTermino.Text, char.Parse(ddlPrioridade.Value.ToString())
                                           , codigoDemandante, codigoUnidade, 14, codigoResponsavel, codigoUsuarioResponsavel, "", ref msgErro);
                msgToShow = resultado ? "As informações do formulário foram salvas" : "Erro ao salvar. " + msgErro;

                callbackSalvar.JSProperties["cp_status"] = resultado ? "ok" : "erro";
            }
            msgToShow = msgToShow.Replace("'", "\"").Replace(Environment.NewLine, ""); // retirando newline pois estava dando erro (G.)
            callbackSalvar.JSProperties["cp_MsgStatus"] = msgToShow;
        }

        protected void ddlDemandante_ItemRequestedByValue(object source, ListEditItemRequestedByValueEventArgs e)
        {
            long value = 0;
            if (e.Value == null || !Int64.TryParse(e.Value.ToString(), out value))
                return;
            ASPxComboBox comboBox = (ASPxComboBox)source;
            dsResponsavel.SelectCommand = cDados.getSQLComboUsuariosPorID(codigoEntidadeUsuarioResponsavel);

            dsResponsavel.SelectParameters.Clear();
            dsResponsavel.SelectParameters.Add("ID", TypeCode.Int64, e.Value.ToString());
            comboBox.DataSource = dsResponsavel;
            comboBox.DataBind();
        }

        protected void ddlDemandante_ItemsRequestedByFilterCondition(object source, ListEditItemsRequestedByFilterConditionEventArgs e)
        {
            ASPxComboBox comboBox = (ASPxComboBox)source;

            string comandoSQL = cDados.getSQLComboUsuarios(codigoEntidadeUsuarioResponsavel, e.Filter, "");

            cDados.populaComboVirtual(dsResponsavel, comandoSQL, comboBox, e.BeginIndex, e.EndIndex);
        }

        protected void ddlResponsavel_ItemRequestedByValue(object source, ListEditItemRequestedByValueEventArgs e)
        {
            long value = 0;
            if (e.Value == null || !Int64.TryParse(e.Value.ToString(), out value))
                return;
            ASPxComboBox comboBox = (ASPxComboBox)source;
            dsResponsavel.SelectCommand = cDados.getSQLComboUsuariosPorID(codigoEntidadeUsuarioResponsavel);

            dsResponsavel.SelectParameters.Clear();
            dsResponsavel.SelectParameters.Add("ID", TypeCode.Int64, e.Value.ToString());
            comboBox.DataSource = dsResponsavel;
            comboBox.DataBind();
        }

        protected void ddlResponsavel_ItemsRequestedByFilterCondition(object source, ListEditItemsRequestedByFilterConditionEventArgs e)
        {
            ASPxComboBox comboBox = (ASPxComboBox)source;

            string comandoSQL = cDados.getSQLComboUsuarios(codigoEntidadeUsuarioResponsavel, e.Filter, "");

            cDados.populaComboVirtual(dsResponsavel, comandoSQL, comboBox, e.BeginIndex, e.EndIndex);
        }

        private void populaCombos()
        {

            //demandante

            ddlDemandante.TextField = "NomeUsuario";
            ddlDemandante.ValueField = "CodigoUsuario";

            ddlDemandante.Columns[0].FieldName = "NomeUsuario";
            ddlDemandante.Columns[1].FieldName = "EMail";
            ddlDemandante.TextFormatString = "{0}";

            //responsável

            ddlResponsavel.TextField = "NomeUsuario";
            ddlResponsavel.ValueField = "CodigoUsuario";

            ddlResponsavel.Columns[0].FieldName = "NomeUsuario";
            ddlResponsavel.Columns[1].FieldName = "EMail";
            ddlResponsavel.TextFormatString = "{0}";

        }
    }

}
