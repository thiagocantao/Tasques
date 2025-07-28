/*
 Alteração:
 
 * 20/10/2010 : Alejandro
                No método [callbackSalvar_Callback], faz uso do outro método [incluiDemandaSimples] da classe 'dados'.
                No método [incluiDemandaSimples] alteré adicionando um novo parâmetro fazendo referencia a tipo de projeto, clasificando
                asim si e de tipo Simple (5: demanda simple) o complexo (4: demanda)
 
 
 */
//todo: consultar com geter o conceito de enviar codigo o descrição no novo parâmetro do metodo [incluiDemandaSimples] fazendo referencia si e demanda simples ou complexa.
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
    public partial class _Projetos_cadastroDemandasSimples : System.Web.UI.Page
    {
        dados cDados;

        private int codigoUsuarioResponsavel;
        private int codigoEntidadeUsuarioResponsavel;
        private int codigoDemanda = -1;
        private ASPxGridView gvToDoList;
        bool podeAdministrar = true;
        bool podeEditarPermissao = false;

        public string alturaPermissoes, urlPermissoes;

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

            if (Request.QueryString["CodigoDemanda"] != null && Request.QueryString["CodigoDemanda"].ToString() != "")
            {
                codigoDemanda = int.Parse(Request.QueryString["CodigoDemanda"].ToString());

                //if (!IsPostBack)
                //{
                //    cDados.VerificaAcessoTela(this, codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoDemanda, "null", "DS", 0, "null", "DS_Cns");
                //}

                podeAdministrar = cDados.VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoDemanda, "null", "DS", 0, "null", "DS_Alt");
                podeEditarPermissao = cDados.VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoDemanda, "null", "DS", 0, "null", "DS_AdmPrs");
            }

            carregaComboUnidades();
            carregaComboStatus();

            hfGeral.Set("TipoOperacao", "Editar");

            dsResponsavel.ConnectionString = cDados.classeDados.getStringConexao();
            dsResponsavel0.ConnectionString = cDados.classeDados.getStringConexao();


        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Header.Controls.Add(cDados.getLiteral(@"<link href=""../estilos/cdisEstilos.css"" rel=""stylesheet"" type=""text/css"" />"));
            Header.Controls.Add(cDados.getLiteral(@"<script type=""text/javascript"" language=""javascript"" src=""../scripts/cadastroDemandaSimples.js""></script>"));
            this.TH(this.TS("cadastroDemandaSimples"));
            carregaComboDemandante();
            carregaComboResponsavel();

            if (codigoDemanda != -1 || hfGeral.Contains("NovoCodigoDemanda"))
            {
                if (codigoDemanda == -1)
                    codigoDemanda = int.Parse(hfGeral.Get("NovoCodigoDemanda").ToString());

                if (!IsPostBack && !IsCallback)
                {
                    montaCampos();
                }


            }
            else
            {
                if (!IsPostBack)
                {

                    tcDemanda.TabPages[1].ClientVisible = false;
                    tcDemanda.TabPages[2].ClientVisible = false;
                    tcDemanda.TabPages[4].ClientVisible = false;
                    tcDemanda.TabPages[5].ClientVisible = false;

                    hfGeral.Set("CodigoResponsavel", "-1");
                    hfGeral.Set("CodigoDemandante", "-1");
                }
            }

            int[] convidados = getParticipantesDemanda();

            Unit tamanho = new Unit(100, UnitType.Percentage);
            int codigoTipoAssociacao = cDados.getCodigoTipoAssociacao("PR");

            int largura = 0;
            int altura = 0;
            string resolucaoCliente = cDados.getInfoSistema("ResolucaoCliente").ToString();

            cDados.getLarguraAlturaTela(resolucaoCliente, out largura, out altura);

            PlanoDeAcao myPlanoDeAcao = null;
            myPlanoDeAcao = new PlanoDeAcao(cDados.classeDados, codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel, codigoDemanda, codigoTipoAssociacao, codigoDemanda, tamanho, 370, !podeAdministrar, convidados.Length == 0 ? null : convidados, true, txtTitulo.Text);
            tcDemanda.TabPages.FindByName("tabTarefas").Controls.AddAt(0, myPlanoDeAcao.constroiInterfaceFormulario());
            gvToDoList = myPlanoDeAcao.gvToDoList;
            gvToDoList.Font.Name = "Verdana";
            gvToDoList.Font.Size = 8;
            gvToDoList.ClientInstanceName = "gvToDoList";
            gvToDoList.ClientSideEvents.BeginCallback = "function(s, e) { hfGeralToDoList.Set('NomeToDoList',  txtTitulo.GetText());}";

            if (!IsCallback)
                gvToDoList.DataBind();

            //cDados.aplicaEstiloVisual(Page);

            DataSet ds = cDados.getDefinicaoUnidade(codigoEntidadeUsuarioResponsavel);
            if (cDados.DataSetOk(ds) && cDados.DataTableOk(ds.Tables[0]))
            {
                lblUnidadeResponsavel.Text = ds.Tables[0].Rows[0]["DescricaoTipoUnidadeNegocio"].ToString() + ":";
                ((ListBoxColumn)ddlUnidade.Columns[1]).Caption = ds.Tables[0].Rows[0]["DescricaoTipoUnidadeNegocio"].ToString();
            }

            urlPermissoes = "../_Estrategias/InteressadosObjeto.aspx?AlturaGrid=310&LarguraGrid=810&TIT=Demandas&ITO=DS&COE=" + codigoDemanda + "&TOE=" + txtTitulo.Text;
            tcDemanda.JSProperties["cp_URL"] = urlPermissoes;

            string urlMSG = "./DadosProjeto/MensagensDemandasSimples.aspx?IDProjeto=" + codigoDemanda;
            tcDemanda.JSProperties["cp_URLMsg"] = urlMSG;

            string urlAnexos = "../espacoTrabalho/frameEspacoTrabalho_BibliotecaInterno.aspx?Popup=S&TA=DS&ID=" + codigoDemanda + "&ALT=425";
            tcDemanda.JSProperties["cp_URLAnexos"] = urlAnexos;

            if (codigoDemanda != -1)
            {
                verificaPermissaoObjetos(podeAdministrar);
            }

            tcDemanda.TabPages[3].ClientVisible = podeEditarPermissao;
        }

        private void verificaPermissaoObjetos(bool habilita)
        {
            txtTitulo.ClientEnabled = habilita;
            txtDetalhes.ClientEnabled = habilita;
            txtInicio.ClientEnabled = habilita;
            txtTermino.ClientEnabled = habilita;
            ddlPrioridade.ClientEnabled = habilita;
            ddlDemandante.ClientEnabled = habilita;
            ddlResponsavel.ClientEnabled = habilita;
            ddlUnidade.ClientEnabled = habilita;
            ddlStatus.ClientEnabled = habilita;

            if (gvToDoList.VisibleRowCount == 0)
                gvToDoList.ClientVisible = habilita;

            ASPxButton1.ClientVisible = habilita;
        }

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
                ddlDemandante.Value = dr["CodigoDemandante"].ToString();
                ddlResponsavel.Value = dr["CodigoGerenteProjeto"].ToString();
                ddlDemandante.Text = dr["NomeDemandante"].ToString();
                ddlResponsavel.Text = dr["NomeGerente"].ToString();
                ddlUnidade.Value = dr["CodigoUnidade"].ToString();
                ddlStatus.Value = dr["CodigoStatusProjeto"].ToString();
                txtComentarios.Html = dr["Comentario"].ToString();

                hfGeral.Set("CodigoResponsavel", dr["CodigoGerenteProjeto"].ToString());
                hfGeral.Set("CodigoDemandante", dr["CodigoDemandante"].ToString());
                hfGeral.Set("codigoObjetoAssociado", codigoDemanda);

                hfGeral.Set("IndiceSelecionadoDemandante", ddlDemandante.Items.IndexOfText(Convert.ToString(dr["NomeDemandante"].ToString())));
                hfGeral.Set("IndiceSelecionadoResponsavel", ddlDemandante.Items.IndexOfText(Convert.ToString(dr["NomeGerente"].ToString())));


                //ListEditItem li = ddlResponsavel.Items.FindByText(dr["NomeGerente"].ToString());
                //ddlResponsavel.SelectedItem = li;

                //ListEditItem li2 = ddlDemandante.Items.FindByText(dr["NomeDemandante"].ToString());
                //ddlDemandante.SelectedItem = li2;

                //int codUnidade = (dr["CodigoUnidade"].ToString() != "" && dr["CodigoUnidade"] != null) ? int.Parse(dr["CodigoUnidade"].ToString()) : -1;
                //ListEditItem li3 = ddlUnidade.Items.FindByValue(codUnidade);
                //ddlUnidade.SelectedItem = li3;


            }
        }

        protected void callbackDemandante_Callback(object source, DevExpress.Web.CallbackEventArgs e)
        {
            string nomeUsuario = "";
            string codigoUsuario = "";
            string where = " AND ";
            where = where + hfGeral.Get("ComandoWhere").ToString();

            cDados.getLov_NomeValor("usuario", "CodigoUsuario", "NomeUsuario", "", false, where, "NomeUsuario", out codigoUsuario, out nomeUsuario);

            // se encontrou um único registro
            if (nomeUsuario != "")
            {
                callbackDemandante.JSProperties["cp_Codigo"] = codigoUsuario;
                callbackDemandante.JSProperties["cp_Nome"] = nomeUsuario;
            }
            else // mostrar popup
            {
                callbackDemandante.JSProperties["cp_Codigo"] = "";
                callbackDemandante.JSProperties["cp_Nome"] = "";
            }
        }

        protected void callbackResponsavel_Callback(object source, DevExpress.Web.CallbackEventArgs e)
        {
            string nomeUsuario = "";
            string codigoUsuario = "";
            string where = " AND ";
            where = where + hfGeral.Get("ComandoWhere").ToString();

            cDados.getLov_NomeValor("usuario", "CodigoUsuario", "NomeUsuario", "", false, where, "NomeUsuario", out codigoUsuario, out nomeUsuario);

            // se encontrou um único registro
            if (nomeUsuario != "")
            {
                callbackResponsavel.JSProperties["cp_Codigo"] = codigoUsuario;
                callbackResponsavel.JSProperties["cp_Nome"] = nomeUsuario;
            }
            else // mostrar popup
            {
                callbackResponsavel.JSProperties["cp_Codigo"] = "";
                callbackResponsavel.JSProperties["cp_Nome"] = "";
            }
        }

        private void carregaComboUnidades()
        {
            string where = string.Format(@" AND {0}.{1}.f_VerificaAcessoConcedido({3}, {2}, CodigoUnidadeNegocio, NULL, 'UN', 0, NULL, 'UN_IncDem') = 1
                    ", cDados.getDbName(), cDados.getDbOwner(), codigoEntidadeUsuarioResponsavel, codigoUsuarioResponsavel);

            DataSet dsUnidades = cDados.getUnidadesNegocioEntidade(codigoEntidadeUsuarioResponsavel, where);

            if (cDados.DataSetOk(dsUnidades))
            {
                ddlUnidade.DataSource = dsUnidades;
                ddlUnidade.DataBind();

                //if (!IsPostBack && ddlUnidade.Items.Count > 0)
                //    ddlUnidade.SelectedIndex = 0;
            }
        }

        protected void callbackSalvar_Callback(object source, DevExpress.Web.CallbackEventArgs e)
        {

        }

        private void carregaComboStatus()
        {
            DataSet dsStatus = cDados.getStatus(" AND TipoStatus = 'DMD'");

            if (cDados.DataSetOk(dsStatus))
            {
                ddlStatus.DataSource = dsStatus;
                ddlStatus.TextField = "DescricaoStatus";
                ddlStatus.ValueField = "CodigoStatus";
                ddlStatus.DataBind();

                if (!IsPostBack && ddlStatus.Items.Count > 0)
                    ddlStatus.SelectedIndex = 0;
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

        protected void ASPxButton1_Click(object sender, EventArgs e)
        {
            int codigoDemandante, codigoResponsavel, codigoUnidade, novoCodigoDemanda = -1;
            string msgErro = "", msgToShow = "";
            bool indicaMsgSucesso = true;

            codigoUnidade = ddlUnidade.SelectedIndex == -1 ? -1 : int.Parse(ddlUnidade.Value.ToString());

            codigoDemandante = int.Parse(hfGeral.Get("CodigoDemandante").ToString());
            codigoResponsavel = int.Parse(hfGeral.Get("CodigoResponsavel").ToString());

            if (codigoDemanda == -1)
            {
                bool resultado = cDados.incluiDemandaSimples(txtTitulo.Text, txtDetalhes.Text, txtInicio.Text, txtTermino.Text, char.Parse(ddlPrioridade.Value.ToString())
                                           , codigoDemandante, codigoUnidade, codigoResponsavel
                                           , codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "5", ref msgErro, ref novoCodigoDemanda);

                msgToShow = resultado ? "Demanda Incluída com Sucesso!" : "Erro ao Incluir a Demanda. " + msgErro;

                indicaMsgSucesso = resultado;

                if (novoCodigoDemanda != -1)
                {

                    callbackSalvar.JSProperties["cp_NovoCodigoDemanda"] = novoCodigoDemanda;
                    hfGeral.Set("NovoCodigoDemanda", novoCodigoDemanda);
                    hfGeral.Set("codigoObjetoAssociado", novoCodigoDemanda);
                    tcDemanda.TabPages.FindByName("tabTarefas").ClientVisible = true;
                    tcDemanda.TabPages.FindByName("tabEncerramento").ClientVisible = true;
                    tcDemanda.TabPages.FindByName("tabPermissoes").ClientVisible = podeEditarPermissao;
                    tcDemanda.TabPages.FindByName("tabMensagens").ClientVisible = true;
                    tcDemanda.TabPages.FindByName("tabAnexos").ClientVisible = true;

                    codigoDemanda = novoCodigoDemanda;

                    montaCampos();
                }
            }
            else
            {

                string resposta = "S";

                //cDados.getPodeReativarDemandaSimples(int.Parse(ddlStatus.Value.ToString()), codigoResponsavel, codigoUnidade, ref resposta);

                if (resposta.Equals("S"))
                {
                    bool resultado = cDados.atualizaDemandaSimples(codigoDemanda, txtTitulo.Text, txtDetalhes.Text, txtInicio.Text, txtTermino.Text, char.Parse(ddlPrioridade.Value.ToString())
                                               , codigoDemandante, codigoUnidade, int.Parse(ddlStatus.Value.ToString()), codigoResponsavel, codigoUsuarioResponsavel, txtComentarios.Html, ref msgErro);

                    msgToShow = resultado ? "Demanda Alterada com Sucesso!" : "Erro ao Alterar a Demanda. " + msgErro;
                    indicaMsgSucesso = resultado;
                }
                else
                {
                    msgToShow = "Erro ao salvar os dados. O responsável por essa demanda é um usuário inativo. A demanda não pode ser gravada com este status!";
                    indicaMsgSucesso = false;
                }
            }

            urlPermissoes = "../_Estrategias/InteressadosObjeto.aspx?AlturaGrid=310&LarguraGrid=810&TIT=Demandas&ITO=DS&COE=" + codigoDemanda + "&TOE=" + txtTitulo.Text;
            tcDemanda.JSProperties["cp_URL"] = urlPermissoes;

            string urlMSG = "./DadosProjeto/MensagensDemandasSimples.aspx?IDProjeto=" + codigoDemanda;
            tcDemanda.JSProperties["cp_URLMsg"] = urlMSG;

            string urlAnexos = "../espacoTrabalho/frameEspacoTrabalho_BibliotecaInterno.aspx?Popup=S&TA=DS&ID=" + codigoDemanda + "&ALT=425";
            tcDemanda.JSProperties["cp_URLAnexos"] = urlAnexos;

            msgToShow = msgToShow.Replace("'", "\"").Replace(Environment.NewLine, ""); // retirando newline pois estava dando erro (G.)
            callbackSalvar.JSProperties["cp_MsgStatus"] = msgToShow;

            string script = "";

            if (indicaMsgSucesso)
                script = "window.top.mostraMensagem('" + msgToShow + "', 'sucesso', false, false, null);";
            else
                script = "window.top.mostraMensagem('" + msgToShow + "', 'erro', true, false, null);";

            ClientScript.RegisterClientScriptBlock(GetType(), "client", "<script type='text/javascript' language='Javascript'>" + script + "</script>", false);

        }

        protected void ddlDemandante_ItemRequestedByValue(object source, ListEditItemRequestedByValueEventArgs e)
        {
            if (e.Value != null)
            {
                long value = 0;
                if (!Int64.TryParse(e.Value.ToString(), out value))
                    return;
                ASPxComboBox comboBox = (ASPxComboBox)source;
                dsResponsavel.SelectCommand = cDados.getSQLComboUsuariosPorID(codigoEntidadeUsuarioResponsavel);

                dsResponsavel.SelectParameters.Clear();
                dsResponsavel.SelectParameters.Add("ID", TypeCode.Int64, e.Value.ToString());
                comboBox.DataSource = dsResponsavel;
                comboBox.DataBind();
            }

        }

        protected void ddlDemandante_ItemsRequestedByFilterCondition(object source, ListEditItemsRequestedByFilterConditionEventArgs e)
        {
            ASPxComboBox comboBox = (ASPxComboBox)source;
            string nomeUsuario = e.Filter;
            if (string.IsNullOrEmpty(e.Filter) && comboBox != null)
                nomeUsuario = comboBox.Text;

            string comandoSQL = cDados.getSQLComboUsuarios(codigoEntidadeUsuarioResponsavel, nomeUsuario, "");

            cDados.populaComboVirtual(dsResponsavel, comandoSQL, comboBox, e.BeginIndex, e.EndIndex);
            if (comboBox.SelectedIndex == -1)
                comboBox.SelectedItem = comboBox.Items.FindByText(nomeUsuario);
        }

        private void carregaComboDemandante()
        {

            ddlDemandante.TextField = "NomeUsuario";
            ddlDemandante.ValueField = "CodigoUsuario";


            ddlDemandante.Columns[0].FieldName = "NomeUsuario";
            ddlDemandante.Columns[1].FieldName = "EMail";
            ddlDemandante.TextFormatString = "{0}";

        }

        private void carregaComboResponsavel()
        {

            ddlResponsavel.TextField = "NomeUsuario";
            ddlResponsavel.ValueField = "CodigoUsuario";


            ddlResponsavel.Columns[0].FieldName = "NomeUsuario";
            ddlResponsavel.Columns[1].FieldName = "EMail";
            ddlResponsavel.TextFormatString = "{0}";

        }

        protected void ddlResponsavel_ItemRequestedByValue(object source, ListEditItemRequestedByValueEventArgs e)
        {
            if (e.Value != null)
            {
                long value = 0;
                if (!Int64.TryParse(e.Value.ToString(), out value))
                    return;
                ASPxComboBox comboBox1 = (ASPxComboBox)source;
                dsResponsavel0.SelectCommand = cDados.getSQLComboUsuariosPorID(codigoEntidadeUsuarioResponsavel);

                dsResponsavel0.SelectParameters.Clear();
                dsResponsavel0.SelectParameters.Add("ID", TypeCode.Int64, e.Value.ToString());
                comboBox1.DataSource = dsResponsavel0;
                comboBox1.DataBind();
            }
        }

        protected void ddlResponsavel_ItemsRequestedByFilterCondition(object source, ListEditItemsRequestedByFilterConditionEventArgs e)
        {
            ASPxComboBox comboBox1 = (ASPxComboBox)source;
            string nomeUsuario = e.Filter;
            if (string.IsNullOrEmpty(e.Filter) && comboBox1 != null)
                nomeUsuario = comboBox1.Text;

            string comandoSQL = cDados.getSQLComboUsuarios(codigoEntidadeUsuarioResponsavel, nomeUsuario, "");

            cDados.populaComboVirtual(dsResponsavel0, comandoSQL, comboBox1, e.BeginIndex, e.EndIndex);

            if (comboBox1.SelectedIndex == -1)
                comboBox1.SelectedItem = comboBox1.Items.FindByText(nomeUsuario);
        }
    }


}
