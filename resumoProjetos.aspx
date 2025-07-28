<%@ Page Language="C#" MasterPageFile="~/novaCdis.master" AutoEventWireup="true" CodeBehind="resumoProjetos.aspx.cs"
    Inherits="BriskPtf._Projetos._Projetos_resumoProjetos" Title="Portal da Estratégia" %>

<%@ MasterType VirtualPath="~/novaCdis.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="AreaTrabalho" runat="Server">
    <table cellpadding="0" cellspacing="0" style="width: 100%">
        <tr>
            <td align="left">
                <table align="left" cellpadding="0" cellspacing="0" class="auto-style2">
                    <tr>
                        <td style="width: 120px"></td>
                        <td>
                            <asp:Label ID="Label1" runat="server" EnableViewState="False" Text="<%$ Resources:traducao, nome_do_projeto_ %>" Width="200px"></asp:Label>
                        </td>
                        <td id="td_FiltroGerente" runat="server" style="padding-left: 5px">
                            <asp:Label ID="lblGerentes" runat="server"
                                Text="<%$ Resources:traducao, resumoProjetos_gerente_ %>" EnableViewState="False" Width="100%"></asp:Label></td>
                        <td id="td_FiltroUnidade" runat="server" style="padding-left: 5px">
                            <asp:Label ID="lblUnidades" runat="server" EnableViewState="False"
                                Text="<%$ Resources:traducao, resumoProjetos_unidade_ %>" Width="135px"></asp:Label></td>
                        <td id="tdFiltroUnidadeAtendimento" runat="server" style="padding-left: 5px">
                            <asp:Label ID="lblUnidadeAtendimento" runat="server" EnableViewState="False"
                                Text="<%$ Resources:traducao, resumoProjetos_unidade_atendimento_ %>" Width="100%"></asp:Label></td>
                        <td id="td_FiltroTipo" runat="server" style="padding-left: 5px">
                            <asp:Label ID="lblCategorias" runat="server"
                                Text="<%$ Resources:traducao, resumoProjetos_categoria_ %>" EnableViewState="False" Width="135px"></asp:Label>
                        </td>
                        <td id="td_filtroStatus" runat="server" style="padding-left: 5px; width: 150px">
                            <asp:Label ID="lblStatus" runat="server" Text="<%# Resources.traducao.resumoProjetos_status_ %>"
                                EnableViewState="False"></asp:Label></td>
                        <td id="td_filtroCR" runat="server" style="padding-left: 5px; width: 150px">
                            <asp:Label ID="lblCR" runat="server" Text="<%$ Resources:traducao, resumoProjetos_CR_ %>"
                                EnableViewState="False"></asp:Label></td>
                        <td style="width: 30px"></td>
                        <td style="width: 30px"></td>
                        <td style="width: 30px"></td>
                        <td style="width: 30px"></td>
                        <td id="tdDisplayLaranja" runat="server" style="width: 30px"></td>
                    </tr>
                    <tr>
                        <td>
                            <table runat="server" id="tbBotoesEdicao" cellpadding="0" cellspacing="0" width="100%">
                                <tr runat="server">
                                    <td runat="server" style="width: 120px; padding-left: 10px;" valign="middle">
                                        <dxm:ASPxMenu ID="menu" runat="server" BackColor="Transparent"
                                            ClientInstanceName="menu" ItemSpacing="0px" OnItemClick="menu_ItemClick"
                                            OnInit="menu_Init">
                                            <Paddings Padding="0px" />
                                            <Items>
                                                <dxm:MenuItem Name="btnIncluir" Text="" ToolTip="Incluir">
                                                    <Image Url="~/imagens/botoes/incluirReg02.png">
                                                    </Image>
                                                </dxm:MenuItem>
                                                <dxm:MenuItem Name="btnExportar" Text="" ToolTip="Exportar">
                                                    <Items>
                                                        <dxm:MenuItem Name="btnXLS" Text="XLS" ToolTip="Exportar para XLS">
                                                            <Image Url="~/imagens/menuExportacao/xls.png">
                                                            </Image>
                                                        </dxm:MenuItem>
                                                        <dxm:MenuItem Name="btnPDF" Text="PDF" ToolTip="Exportar para PDF">
                                                            <Image Url="~/imagens/menuExportacao/pdf.png">
                                                            </Image>
                                                        </dxm:MenuItem>
                                                        <dxm:MenuItem Name="btnRTF" Text="RTF" ToolTip="Exportar para RTF">
                                                            <Image Url="~/imagens/menuExportacao/rtf.png">
                                                            </Image>
                                                        </dxm:MenuItem>
                                                        <dxm:MenuItem Name="btnHTML" Text="HTML" ToolTip="Exportar para HTML"
                                                            ClientVisible="False">
                                                            <Image Url="~/imagens/menuExportacao/html.png">
                                                            </Image>
                                                        </dxm:MenuItem>
                                                    </Items>
                                                    <Image Url="~/imagens/botoes/btnDownload.png">
                                                    </Image>
                                                </dxm:MenuItem>
                                                <dxm:MenuItem Name="btnLayout" Text="" ClientVisible="false" ToolTip="Layout">
                                                    <Items>
                                                        <dxm:MenuItem Text="Salvar" ToolTip="Salvar Layout">
                                                            <Image IconID="save_save_16x16">
                                                            </Image>
                                                        </dxm:MenuItem>
                                                        <dxm:MenuItem Text="Restaurar" ToolTip="Restaurar Layout">
                                                            <Image IconID="actions_reset_16x16">
                                                            </Image>
                                                        </dxm:MenuItem>
                                                    </Items>
                                                    <Image Url="~/imagens/botoes/layout.png">
                                                    </Image>
                                                </dxm:MenuItem>
                                            </Items>
                                            <ItemStyle>
                                                <Paddings Padding="2px" />
                                            </ItemStyle>
                                        </dxm:ASPxMenu>
                                        <asp:CheckBox ID="ckProgramas" runat="server" Checked="true" AutoPostBack="False"
                                            onClick="tlProjetos.PerformCallback();" Visible="false" EnableViewState="False" Text="<%$ Resources:traducao, ver_programas %>" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td>
                            <dxe:ASPxTextBox ID="txtDescricao" runat="server"
                                ClientInstanceName="txtDescricao"
                                Width="100%" ToolTip="Informe parte do nome do Projeto para a pesquisa."
                                EnableViewState="False">
                                <ClientSideEvents KeyDown="function(s, e) {
                                                if(e.htmlEvent.keyCode == 13) {
                                                    ASPxClientUtils.PreventEventAndBubble(e.htmlEvent);
                                                }
                                                else{
                                                    novaDescricao();	
                                                }
                                            }" />
                            </dxe:ASPxTextBox>
                        </td>
                        <td id="td_FiltroGerente1" runat="server" style="padding-left: 5px">
                            <dxe:ASPxComboBox ID="ddlGerentes" runat="server"
                                ClientInstanceName="ddlGerentes" ValueType="System.Int32" Width="100%"
                                EnableViewState="False"
                                OnItemRequestedByValue="ddlGerentes_ItemRequestedByValue"
                                OnItemsRequestedByFilterCondition="ddlGerentes_ItemsRequestedByFilterCondition"
                                IncrementalFilteringMode="Contains" TextFormatString="{0}"
                                CallbackPageSize="50" EnableCallbackMode="True">
                                <ClientSideEvents SelectedIndexChanged="function(s, e) {
	                                            mudaCorComboDev(ddlGerentes, 'lblGerentes', '-1'); 
	                                            tlProjetos.PerformCallback();
                                            }"
                                    Init="function(s, e) {
	                                            mudaCorComboDev(ddlGerentes, 'lblGerentes', '-1'); 
                                            }"></ClientSideEvents>
                                <Columns>
                                    <dxe:ListBoxColumn Width="280px" Caption="Nome"></dxe:ListBoxColumn>
                                    <dxe:ListBoxColumn Width="300px" Caption="Email"></dxe:ListBoxColumn>
                                </Columns>
                            </dxe:ASPxComboBox>
                        </td>
                        <td id="td_FiltroUnidade1" runat="server" style="padding-left: 5px">
                            <dxe:ASPxComboBox ID="ddlUnidades" runat="server"
                                ClientInstanceName="ddlUnidades" ValueType="System.Int32" Width="100%"
                                EnableViewState="False"
                                IncrementalFilteringMode="Contains" TextFormatString="{0}"
                                CallbackPageSize="50" EnableCallbackMode="True">
                                <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlUnidades, 'lblUnidades', '-1'); 
	tlProjetos.PerformCallback();
}"
                                    Init="function(s, e) {
	mudaCorComboDev(ddlUnidades, 'lblUnidades', '-1'); 
	mudaCorCheck('checkVerde', 'lblVerde', false); 
	mudaCorCheck('checkAmarelo', 'lblAmarelo', false); 
	mudaCorCheck('checkVermelho', 'lblVermelho', false); 
}"></ClientSideEvents>
                                <Columns>
                                    <dxe:ListBoxColumn Width="150px" Caption="Sigla"></dxe:ListBoxColumn>
                                    <dxe:ListBoxColumn Width="400px" Caption="Unidade"></dxe:ListBoxColumn>
                                </Columns>
                            </dxe:ASPxComboBox>
                        </td>
                        <td id="tdFiltroUnidadeAtendimento1" runat="server" style="padding-left: 5px">
                            <dxe:ASPxComboBox ID="ddlUnidadeAtendimento" runat="server"
                                ClientInstanceName="ddlUnidadeAtendimento" ValueType="System.Int32"
                                Width="100%" EnableViewState="False"
                                IncrementalFilteringMode="Contains" TextFormatString="{0}"
                                CallbackPageSize="50" EnableCallbackMode="True">
                                <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlUnidadeAtendimento, 'lblUnidadeAtendimento', '-1'); 
	tlProjetos.PerformCallback();
}"
                                    Init="function(s, e) {
	mudaCorComboDev(ddlUnidadeAtendimento, 'lblUnidadeAtendimento', '-1'); 
	mudaCorCheck('checkVerde', 'lblVerde', false); 
	mudaCorCheck('checkAmarelo', 'lblAmarelo', false); 
	mudaCorCheck('checkVermelho', 'lblVermelho', false); 
}"></ClientSideEvents>
                                <Columns>
                                    <dxe:ListBoxColumn Width="100px" Caption="Sigla"></dxe:ListBoxColumn>
                                    <dxe:ListBoxColumn Width="150px" Caption="Unidade"></dxe:ListBoxColumn>
                                </Columns>
                            </dxe:ASPxComboBox>
                        </td>
                        <td id="td_FiltroTipo1" runat="server" style="padding-left: 5px">
                            <dxe:ASPxComboBox ID="ddlCategoria" runat="server"
                                ClientInstanceName="ddlCategoria" ShowShadow="False"
                                Width="100%" CallbackPageSize="50"
                                EnableCallbackMode="True">
                                <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlCategoria, 'lblCategorias', '-1'); 
	tlProjetos.PerformCallback();
}"
                                    Init="function(s, e) {
	mudaCorComboDev(ddlCategoria, 'lblCategorias', '-1'); 
}" />
                                <Columns>
                                    <dxe:ListBoxColumn Caption="Categoria" Width="200px" />
                                    <dxe:ListBoxColumn Caption="Sigla" Width="100px" />
                                </Columns>
                            </dxe:ASPxComboBox>
                        </td>
                        <td id="td_filtroStatus1" runat="server" style="padding-left: 5px; width: 150px">
                            <dxe:ASPxComboBox ID="ddlStatus" runat="server" ClientInstanceName="ddlStatus"
                                Width="100%" EnableViewState="False">
                                <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlStatus, 'lblStatus', '3');   
	tlProjetos.PerformCallback();
}"
                                    Init="function(s, e) {
	mudaCorComboDev(ddlStatus, 'lblStatus', '3');   
}" />
                                <ItemStyle Wrap="True" />
                            </dxe:ASPxComboBox>
                        </td>
                        <td id="td_filtroCR1" runat="server" style="padding-left: 5px; width: 150px">
                            <dxe:ASPxComboBox ID="ddlCR" runat="server" ClientInstanceName="ddlCR"
                                Width="100%" EnableViewState="False">
                                <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlCR, 'lblCR', '-1');   
	tlProjetos.PerformCallback();
}"
                                    Init="function(s, e) {
	mudaCorComboDev(ddlCR, 'lblCR', '-1');   
}" />
                                <ItemStyle Wrap="True" />
                            </dxe:ASPxComboBox>
                        </td>
                        <td style="width: 30px; padding-left: 5px">
                            <table cellpadding="0" cellspacing="0" style="width: 30px">
                                <tr>
                                    <td align="right">
                                        <asp:CheckBox ID="checkVerde" runat="server" AutoPostBack="False" Checked="True"
                                            onClick="mudaCorCheck('checkVerde', 'lblVerde', false); setTimeout ('tlProjetos.PerformCallback();', 1000);" EnableViewState="False" Width="100%" /></td>
                                    <td align="left">
                                        <img alt="" src="../imagens/verde.gif" /></td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 30px; padding-left: 5px">
                            <table cellpadding="0" cellspacing="0" style="width: 30px">
                                <tr>
                                    <td style="width: 20px" align="right">
                                        <asp:CheckBox ID="checkAmarelo" runat="server" AutoPostBack="False" Checked="True"
                                            onClick="mudaCorCheck('checkAmarelo', 'lblAmarelo', false); setTimeout ('tlProjetos.PerformCallback();', 1000);" EnableViewState="False" Width="100%" /></td>
                                    <td align="left">
                                        <img alt="" src="../imagens/amarelo.gif" /></td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 30px; padding-left: 5px">
                            <table cellpadding="0" cellspacing="0" style="width: 30px">
                                <tr>
                                    <td style="width: 20px" align="right">
                                        <asp:CheckBox ID="checkVermelho" runat="server" AutoPostBack="False" Checked="True"
                                            onClick="mudaCorCheck('checkVermelho', 'lblVermelho', false); setTimeout ('tlProjetos.PerformCallback();', 1000);" EnableViewState="False" Width="100%" /></td>
                                    <td align="left">
                                        <img alt="" src="../imagens/vermelho.gif" /></td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 30px; padding-left: 5px">
                            <table cellpadding="0" cellspacing="0" style="width: 30px">
                                <tr>
                                    <td style="width: 20px" align="right">
                                        <asp:CheckBox ID="checkBranco" runat="server" AutoPostBack="False" Checked="True"
                                            onClick="mudaCorCheck('checkBranco', 'lblBranco', false); setTimeout ('tlProjetos.PerformCallback();', 1000);" EnableViewState="False" Width="100%" /></td>
                                    <td align="left">
                                        <img alt="" src="../imagens/branco.gif" /></td>
                                </tr>
                            </table>
                        </td>
                        <td id="tdDisplayLaranja1" runat="server" style="width: 30px; padding-left: 5px; padding-right: 5px">
                            <table cellpadding="0" cellspacing="0" style="width: 30px">
                                <tr>
                                    <td style="width: 20px" align="right">
                                        <asp:CheckBox ID="checkLaranja" runat="server" AutoPostBack="False" Checked="True"
                                            onClick="mudaCorCheck('checkLaranja', 'lblLaranja', false); setTimeout ('tlProjetos.PerformCallback();', 1000);" EnableViewState="False" Width="100%" /></td>
                                    <td align="left">
                                        <img alt="" src="../imagens/laranja.gif" /></td>
                                </tr>
                            </table>
                        </td>

                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>
                <div id="divGrid" style="visibility: hidden; padding-top: 5px; padding-left: 10px; padding-right: 5px">
                    <dxwtl:ASPxTreeList ID="tlProjetos" runat="server" AutoGenerateColumns="False"
                        KeyFieldName="Codigo"
                        ParentFieldName="CodigoProjetoPai" ClientInstanceName="tlProjetos"
                        OnCustomCallback="tlProjetos_CustomCallback"
                        OnHtmlDataCellPrepared="tlProjetos_HtmlDataCellPrepared" Width="100%">
                        <ClientSideEvents EndCallback="function(s, e) {
	document.getElementById('checkVerde').disabled = false;
	document.getElementById('checkAmarelo').disabled = false;
	document.getElementById('checkVermelho').disabled = false;
	document.getElementById('checkLaranja').disabled = false;
    var height = Math.max(0, document.documentElement.clientHeight) - 200;
    s.SetHeight(height);
    document.getElementById('divGrid').style.visibility = '';
}"
                            Init="function(s, e) 
                                                    {
                                                    var height = Math.max(0, document.documentElement.clientHeight) - 200;
                                                    s.SetHeight(height);
                                                    document.getElementById('divGrid').style.visibility = '';
                                                    }" />
                        <Settings SuppressOuterGridLines="True" VerticalScrollBarMode="Visible"></Settings>
                        <Columns>
                            <dxwtl:TreeListTextColumn Caption="Descri&#231;&#227;o" FieldName="Descricao"
                                VisibleIndex="0" Width="350px">
                                <DataCellTemplate>
                                    <%# getDescricaoObjetosLista()%>
                                </DataCellTemplate>
                                <HeaderStyle HorizontalAlign="Left" />
                                <CellStyle HorizontalAlign="Left">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Unidade Atendimento"
                                FieldName="NomeUnidadeAtendimento" Name="NomeUnidadeAtendimento"
                                VisibleIndex="1" Width="180px" ExportWidth="180">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Gerente" FieldName="GerenteProjeto" Name="Gerente"
                                VisibleIndex="2" Width="210px" ExportWidth="210">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Categoria" FieldName="TipoProjeto"
                                Name="Tipo" VisibleIndex="3" Width="180px" ExportWidth="180">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Geral" FieldName="CorGeral" Name="Geral" VisibleIndex="4"
                                Width="200" ExportWidth="200">
                                <PropertiesTextEdit DisplayFormatString="&lt;img src='../imagens/{0}.gif' style='border-width:0px;' /&gt;">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Risco" FieldName="CorRisco" Name="Risco"
                                VisibleIndex="5" Width="150px" ExportWidth="150">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;"
                                    NullDisplayText=" " NullText=" ">
                                    <Style Font-Overline="False" Font-Underline="False">
                                    </Style>
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" Font-Overline="False"
                                    Font-Strikeout="False" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="F&#237;sico" FieldName="CorFisico" Name="Fisico"
                                VisibleIndex="6" Width="200" ExportWidth="200">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Despesa" FieldName="CorFinanceiro" Name="Custo"
                                VisibleIndex="7" Width="70px" ExportWidth="80">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Escopo" FieldName="CorEscopo" Name="Escopo"
                                VisibleIndex="8" Width="55px" ExportWidth="55">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Receita" FieldName="CorReceita" Name="CorReceita"
                                VisibleIndex="9" Width="55px" ExportWidth="55">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Trabalho" FieldName="corTrabalho"
                                Name="Trabalho" VisibleIndex="10" Width="65px" MinWidth="65">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Meta" FieldName="corMeta" Name="Meta"
                                VisibleIndex="11" Width="50px" ExportWidth="50">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="IAM" FieldName="corIAM" Name="IAM"
                                VisibleIndex="12" Width="40px" ExportWidth="40">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;">
                                </PropertiesTextEdit>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Convênio" FieldName="corConvenio" Name="Convenio"
                                VisibleIndex="13" Width="65px" ExportWidth="55">
                                <PropertiesTextEdit DisplayFormatString="&lt;img style='border:0px' src='../imagens/{0}.gif'/&gt;">
                                </PropertiesTextEdit>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Status" FieldName="StatusProjeto" Name="Status"
                                VisibleIndex="14" Width="140px" ExportWidth="140">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Última Atualização"
                                FieldName="UltimaAtualizacao" Name="UltimaAtualizacao" VisibleIndex="15"
                                Width="150px" ExportWidth="150">
                                <PropertiesTextEdit ConvertEmptyStringToNull="False"
                                    NullDisplayText="--/--/----" NullText="--/--/----">
                                </PropertiesTextEdit>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="CR" FieldName="CR"
                                Name="CR" VisibleIndex="16" Width="180px" ExportWidth="180">
                            </dxwtl:TreeListTextColumn>
                            <dxwtle:TreeListTextColumn AutoFilterCondition="Default" Caption="corConvenio" FieldName="corConvenio" Name="corConvenio" ShowInFilterControl="Default" VisibleIndex="16">
                            </dxwtle:TreeListTextColumn>
                            <dxwtle:TreeListTextColumn AutoFilterCondition="Default" Caption="CodigoProjeto" FieldName="CodigoProjeto" Name="CodigoProjeto" ShowInFilterControl="Default" VisibleIndex="17" Visible="False">
                            </dxwtle:TreeListTextColumn>
                        </Columns>
                        <Settings SuppressOuterGridLines="True" VerticalScrollBarMode="Visible"
                            HorizontalScrollBarMode="Auto" />
                        <SettingsResizing ColumnResizeMode="Control" />
                        <SettingsPager PageSize="50" Mode="ShowPager">
                        </SettingsPager>
                        <SettingsCustomizationWindow Caption="<%$ Resources:traducao, resumoProjetos_seletor_de_campos %>" Enabled="True"
                            PopupHeight="220px" PopupWidth="250px" />
                        <SettingsText CustomizationWindowCaption="<%$ Resources:traducao, resumoProjetos_seletor_de_campos %>" />
                        <Styles>
                            <Cell Wrap="True">
                            </Cell>
                        </Styles>
                        <ClientSideEvents BeginCallback="function(s, e) {
}"
                            EndCallback="function(s, e) {
}"
                            ContextMenu="treeList_ContextMenu"></ClientSideEvents>
                        <Templates>
                            <Preview>
                                <table cellspacing="0" cellpadding="0">
                                    <tbody>
                                        <tr>
                                            <td style="width: 25px">
                                                <img src="../imagens/verdeMenor.gif" /></td>
                                            <td style="width: 75px">
                                                <span style="">
                                                    <asp:Label ID="lblVerde" runat="server" EnableViewState="False"
                                                        Font-Size="7pt" Text="<%$ Resources:traducao, txt_ind_desemp_satisfatorio %>"></asp:Label>
                                                </span>
                                            </td>
                                            <td style="width: 25px">
                                                <img src="../imagens/amareloMenor.gif" /></td>
                                            <td style="width: 55px">
                                                <span style="">
                                                    <asp:Label ID="lblAmarelo" runat="server" EnableViewState="False"
                                                        Font-Size="7pt" Text="<%$ Resources:traducao, txt_ind_desemp_atencao %>"></asp:Label>
                                                </span>
                                            </td>
                                            <td style="width: 25px">
                                                <img src="../imagens/vermelhoMenor.gif" /></td>
                                            <td style="width: 45px">
                                                <span style="">
                                                    <asp:Label ID="lblVermelho" runat="server" EnableViewState="False"
                                                        Font-Size="7pt" Text="<%$ Resources:traducao, txt_ind_desemp_critico %>"></asp:Label>
                                                </span>
                                            </td>
                                            <td style="width: 125px" valign="middle">
                                                <span>
                                                    <table style="width: 100%" cellspacing="0" cellpadding="0">
                                                        <tbody>
                                                            <tr>
                                                                <td>&nbsp;</td>
                                                                <td style="height: 13px">&nbsp;<asp:Label ID="lblFiltrosAplicados" runat="server" EnableViewState="False"
                                                                    Text="<%$ Resources:traducao, resumoProjetos_filtros_aplicados %>"></asp:Label>
                                                                    <span></span>
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </span>
                                            </td>
                                            <td style="width: 27px" align="center">
                                                <asp:Image ID="Image1" runat="server" EnableViewState="False" ImageUrl="~/imagens/projetoMenor.PNG" />
                                            </td>
                                            <td style="width: 145px">
                                                <asp:Label ID="lblProjetosSubProjetos" runat="server" EnableViewState="False"
                                                    Font-Size="7pt" Text="<%$ Resources:traducao, projetos_e_subprojetos %>"></asp:Label>
                                            </td>
                                            <td style="width: 29px" align="center">
                                                <asp:Image ID="imgProgramas" runat="server" EnableViewState="False" ImageUrl="~/imagens/programaMenor.PNG" />
                                            </td>
                                            <td style="width: 75px">
                                                <asp:Label ID="lblProgramas" runat="server" EnableViewState="False"
                                                    Font-Size="7pt" Text="<%$ Resources:traducao, programas %>"></asp:Label>
                                            </td>
                                            <td style="width: 295px">
                                                <span><span><span style="text-decoration: underline"></span>
                                                    <asp:Label ID="lblSublinhado" runat="server" EnableViewState="False"
                                                        Font-Size="7pt" Font-Strikeout="True" Font-Underline="False" Text="<%$ Resources:traducao, tachados_ %>"></asp:Label>
                                                    <asp:Label ID="lblSublinhadoLegenda" runat="server" EnableViewState="False"
                                                        Font-Size="7pt" Text="<%$ Resources:traducao, n_o_comp_e_o_sinalizador_geral %>"></asp:Label>
                                                </span></span>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </Preview>
                        </Templates>
                    </dxwtl:ASPxTreeList>
                </div>
            </td>
        </tr>
        <tr>
            <td style="padding-left: 10px; padding-top: 5px">
                <span>
                    <a href="#" onclick="openSidenav('mySidenavLegend')" style="position: relative; background: #617580; color: #fff !important; font-size: 12px; padding: 5px;">
                        <i class="fas fa-list-ul" style="padding-right: 10px;"></i>
                        <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_legenda %>" />
                    </a>
                </span>
                <div id="mySidenavLegend" class="sidenav">
                    <a href="javascript:void(0)" class="closebtn" onclick="closeSidenav('mySidenavLegend')">&times;</a>
                    <div class="colPerfil">
                        <h1 class="legend-title">
                            <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_legendas %>" /></h1>
                        <ul>
                            <li><a style="color: #5e585c;"><i class="fas fa-circle ic-legend ic-verde"></i>
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_satisfat_rio %>" /></a></li>
                            <li><a style="color: #5e585c;"><i class="fas fa-circle ic-legend ic-amarelo"></i>
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_em_aten__o %>" /></a></li>
                            <li><a style="color: #5e585c;"><i class="fas fa-circle ic-legend ic-vermelho"></i>
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_cr_tico %>" /></a></li>
                            <li><a style="color: #5e585c;"><i class="fas fa-circle ic-legend ic-cinza"></i>
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_sem_informa__o %>" /></a></li>
                            <li><a style="color: #5e585c;"><i class="fas fa-circle ic-legend ic-azul"></i>
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_encerrados %>" /></a></li>
                            <li><a style="color: #5e585c;">
                                <img src="../imagens/projeto.PNG" width="16" style="margin-right: 7px;" />
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_projetos_e_subprojetos %>" /></a></li>
                            <li><a style="color: #5e585c;">
                                <img src="../imagens/programa.PNG" width="16" style="margin-right: 7px;" />
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_programas %>" /></a></li>
                            <li><a style="color: #5e585c;">
                                <img src="../imagens/processos.PNG" width="16" style="margin-right: 7px;" />
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_processos %>" /></a></li>
                            <li><a style="color: #5e585c;">
                                <img src="../imagens/agile.PNG" width="16" style="margin-right: 7px;" />
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_projetos__geis %>" />
                            </a></li>
                            <li><a style="color: #5e585c;">
                                <img src="../imagens/sprint.PNG" width="16" style="margin-right: 7px;" />
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_sprints %>" /></a></li>
                            <li><a style="color: #5e585c;">
                                <img src="../imagens/menu-portfolio.PNG" width="16" style="margin-right: 7px;" />
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_portf_lios %>" /></a></li>
                            <li><a style="color: #5e585c;">
                                <img src="../imagens/Logo-GDH.PNG" width="16" style="margin-right: 7px;" />
                                <asp:Literal runat="server" Text="Demandas GDH" /></a></li>
                            <li><a style="color: #5e585c; text-decoration: line-through;">
                                <asp:Literal runat="server" Text="<%$ Resources:traducao, resumoProjetos_n_o_comp_e_o_sinalizador_geral %>" /></a></li>
                        </ul>
                    </div>
                </div>
            </td>
        </tr>
    </table>
    <dxhf:ASPxHiddenField ID="hfGeral" runat="server" ClientInstanceName="hfGeral">
    </dxhf:ASPxHiddenField>
    <dxwtle:ASPxTreeListExporter ID="ASPxTreeListExporter1" runat="server"
        OnRenderBrick="ASPxTreeListExporter1_RenderBrick">
        <Settings ExportAllPages="True">
        </Settings>
        <Styles>
            <Header Font-Bold="True">
            </Header>
            <Cell HorizontalAlign="Left">
            </Cell>
        </Styles>
    </dxwtle:ASPxTreeListExporter>
    <dxm:ASPxPopupMenu ID="pmColumnMenu" runat="server" ClientInstanceName="pmColumnMenu">
        <Items>
            <dxm:MenuItem Name="cmdShowCustomization" Text="<%$ Resources:traducao, resumoProjetos_selecionar_campos %>">
            </dxm:MenuItem>
        </Items>
        <ClientSideEvents ItemClick="function(s, e) {
	if(e.item.name == 'cmdShowCustomization'){
        tlProjetos.ShowCustomizationWindow();
}
}" />
    </dxm:ASPxPopupMenu>
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        .auto-style2 {
            width: 100%;
            float: right;
            padding-left: 5px;
        }
    </style>
</asp:Content>

