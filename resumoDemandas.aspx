<%@ Page Language="C#" MasterPageFile="~/novaCdis.master" AutoEventWireup="true" CodeBehind="resumoDemandas.aspx.cs"
    Inherits="BriskPtf._Projetos._Projetos_resumoDemandas" Title="Portal da Estratégia" %>

<%@ MasterType VirtualPath="~/novaCdis.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="AreaTrabalho" EnableViewState="false" runat="Server">
    <div enableviewstate="false">
        <table border="0" cellpadding="0" cellspacing="0" style="width: 100%; background-image: url(../imagens/titulo/back_Titulo_Desktop.gif);">
            <tr height="26px">
                <td valign="middle" style="padding-left: 10px">
                    <asp:Label ID="lblTituloTela" runat="server" Font-Bold="True"
                        Font-Overline="False" Font-Strikeout="False" Text="Demandas da Instituição"
                        EnableViewState="False"></asp:Label></td>
                <td align="left" valign="middle">
                </td>
        </table>
        <!-- Painel arrendondado lista de projetos -->
    </div>
    <table cellpadding="0" cellspacing="0" enableviewstate="false">
        <tr>
            <td align="right" style="width: 5px; height: 50px"></td>
            
            <td align="right">
                <table border="0" cellpadding="0" cellspacing="0" style=" width: 100%;" id="tbFiltro">
                    <tr>
                        <td align="center" style="width: 40px" valign="bottom">
                            <dxe:ASPxImage ID="imgNovaDemanda" runat="server" Cursor="pointer" ImageUrl="~/imagens/botoes/demandaSimplesI.PNG"
                                ToolTip="Incluir Nova Demanda Simples">
                                <ClientSideEvents Click="function(s, e) {
	var url = &quot;./cadastroDemandasSimples.aspx&quot;;
	
	window.top.showModal(url, 'Inclusão de Demanda', 840, 590, atualizaGrid, null);
     
}" />
                            </dxe:ASPxImage>
                        </td>
                        <td align="left" valign="bottom">
                            <dxe:ASPxImage ID="ASPxImage1" runat="server" Cursor="pointer" ImageUrl="~/imagens/botoes/demandaComplexaI.PNG"
                                ToolTip="Incluir Nova Demanda Complexa">
<ClientSideEvents Click="function(s, e) {
	callback.PerformCallback(); 
}"></ClientSideEvents>
</dxe:ASPxImage>
                        </td>
                        <td align="left">
                            <table>
                                <tr>
                                    <td>
                                        <asp:Label ID="Label1" runat="server" EnableViewState="False"
                                            Text="Demanda:"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <dxe:ASPxTextBox ID="txtDescricao" runat="server" ClientInstanceName="txtDescricao"
                                            EnableViewState="False"  ToolTip="Informe parte do nome do Projeto para a pesquisa."
                                            Width="99%">
                                            <ClientSideEvents KeyDown="function(s, e) {
                                            novaDescricao();	
                                            }" />
                                        </dxe:ASPxTextBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 185px;" id="td_FiltroGerente" align="left">
                            <table>
                                <tr>
                                    <td class="subMenu">
                                        <asp:Label ID="lblGerentes" runat="server"  Text="Responsável:" EnableViewState="False"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <dxe:ASPxComboBox ID="ddlGerentes" runat="server" ClientInstanceName="ddlGerentes" ValueType="System.String" Width="180px" EnableViewState="False"  IncrementalFilteringMode="Contains">
                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	                                            mudaCorComboDev(ddlGerentes, 'lblGerentes', '-1'); 
	                                            tlProjetos.PerformCallback();
                                            }" Init="function(s, e) {
	                                            mudaCorComboDev(ddlGerentes, 'lblGerentes', '-1'); 
                                            }" />
                                            <Columns>
                                                <dxe:ListBoxColumn Caption="Nome" Width="300px" />
                                                <dxe:ListBoxColumn Caption="Email" Width="200px" />
                                            </Columns>
                                        </dxe:ASPxComboBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td align="left" style="width: 190px">
                            <table>
                                <tr>
                                    <td class="subMenu">
                                        <asp:Label ID="lblDemandante" runat="server" EnableViewState="False"
                                            Text="Demandante:"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <dxe:ASPxComboBox ID="ddlDemandante" runat="server" ClientInstanceName="ddlDemandante" ValueType="System.String" Width="185px" EnableViewState="False"  IncrementalFilteringMode="Contains">
                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	                                            mudaCorComboDev(ddlDemandante, 'lblDemandante', '-1'); 
	                                            tlProjetos.PerformCallback();
                                            }" Init="function(s, e) {
	                                            mudaCorComboDev(ddlDemandante, 'lblDemandante', '-1'); 
                                            }" />
                                            <Columns>
                                                <dxe:ListBoxColumn Caption="Nome" Width="300px" />
                                                <dxe:ListBoxColumn Caption="Email" Width="200px" />
                                            </Columns>
                                        </dxe:ASPxComboBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 165px;" id="td_FiltroUnidade" align="left">
                            <table>
                                <tr>
                                    <td class="subMenu">
                                        <asp:Label ID="lblUnidades" runat="server" EnableViewState="False" 
                                            Text="Unidade:"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <dxe:ASPxComboBox ID="ddlUnidades" runat="server" ClientInstanceName="ddlUnidades" ValueType="System.String" Width="160px" EnableViewState="False"  IncrementalFilteringMode="Contains">
                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlUnidades, 'lblUnidades', '-1'); 
	tlProjetos.PerformCallback();
}" Init="function(s, e) {
	mudaCorComboDev(ddlUnidades, 'lblUnidades', '-1'); 
}" />
                                            <Columns>
                                                <dxe:ListBoxColumn Caption="Sigla" Width="100px" />
                                                <dxe:ListBoxColumn Caption="Nome" Width="250px" />
                                            </Columns>
                                        </dxe:ASPxComboBox>
                                    </td>
                                </tr>
                            </table>
                        </td>                        
                        <td style="width: 200px" align="left">
                            <table>
                                <tr>
                                    <td class="subMenu">
                                        <asp:Label ID="lblStatus" runat="server"  Text="Status:"
                                            EnableViewState="False"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <dxe:ASPxComboBox ID="ddlStatus" runat="server" ClientInstanceName="ddlStatus" ValueType="System.String"
                                            Width="195px" EnableViewState="False" >
                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlStatus, 'lblStatus', s.cp_ValorDefault);   
	tlProjetos.PerformCallback();
}" Init="function(s, e) {
	mudaCorComboDev(ddlStatus, 'lblStatus', s.cp_ValorDefault);   
}" />
                                        </dxe:ASPxComboBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td align="left" style="width: 70px">
                            <table>
                                <tr>
                                    <td class="subMenu">
                                        <asp:Label ID="lblPrioridade" runat="server" EnableViewState="False"
                                            Text="Prioridade:"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <dxe:ASPxComboBox ID="ddlPrioridade" runat="server" ClientInstanceName="ddlPrioridade" ValueType="System.String"
                                            Width="70px" EnableViewState="False" SelectedIndex="0" >
                                            <Items>
                                                <dxe:ListEditItem Selected="True" Text="Todas" Value="-1" />
                                                <dxe:ListEditItem Text="Alta" Value="A" />
                                                <dxe:ListEditItem Text="M&#233;dia" Value="M" />
                                                <dxe:ListEditItem Text="Baixa" Value="B" />
                                            </Items>
                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlPrioridade, 'lblPrioridade', '-1');    
	tlProjetos.PerformCallback();
}" Init="function(s, e) {
	mudaCorComboDev(ddlPrioridade, 'lblPrioridade', '-1');   
}" />
                                        </dxe:ASPxComboBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td align="center"></td>
                    </tr>
                </table>
            </td>
            
            
        </tr>
        <tr>
            <td style="width: 5px">
            </td>
            <td>
                    <dxwtl:aspxtreelist id="tlProjetos" runat="server" autogeneratecolumns="False" 
                        keyfieldname="Codigo" parentfieldname="CodigoProjetoPai" 
                        ClientInstanceName="tlProjetos" OnCustomCallback="tlProjetos_CustomCallback" 
                        OnHtmlDataCellPrepared="tlProjetos_HtmlDataCellPrepared" Width="100%" 
                        EnableViewState="False"  >
                        <Columns>
                            <dxwtl:TreeListTextColumn Caption="Descri&#231;&#227;o" FieldName="Descricao" Name="Descricao"
                                VisibleIndex="0">
                                <DataCellTemplate>
                                    <%# (Eval("IndicaProjeto").ToString() == "S") ? "<table><tr><td>&nbsp;<a class='LinkGrid' href='#' onclick='abreResumoDemanda(" + Eval("CodigoProjeto").ToString() + ",\"" + Eval("Descricao").ToString().Replace("'", " ") + "\",\"" + Eval("TipoDemanda").ToString() + "\");'>" + Eval("Descricao") + "</a></td></tr></table>" : "<table><tr><td>" + Eval("Descricao").ToString() + "</td></tr></table>"%>
                                </DataCellTemplate>
                                <HeaderStyle HorizontalAlign="Left" />
                                <CellStyle HorizontalAlign="Left">
                                    <Paddings PaddingLeft="1px" />
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Respons&#225;vel" FieldName="GerenteProjeto" Name="Gerente"
                                VisibleIndex="1" Width="180px">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Demandante" FieldName="Demandante" VisibleIndex="2"
                                Width="180px">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="In&#237;cio" FieldName="InicioProposta" VisibleIndex="3"
                                Width="100px">
                                <PropertiesTextEdit DisplayFormatString="{0:dd/MM/yyyy}">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="T&#233;rmino" FieldName="TerminoProposta" VisibleIndex="4"
                                Width="100px">
                                <PropertiesTextEdit DisplayFormatString="{0:dd/MM/yyyy}">
                                </PropertiesTextEdit>
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Status" FieldName="StatusProjeto" Name="Status"
                                VisibleIndex="5" Width="165px">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Priorid." FieldName="Prioridade" VisibleIndex="6"
                                Width="50px">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Tipo" FieldName="TipoDemanda" VisibleIndex="7"
                                Width="50px">
                                <HeaderStyle HorizontalAlign="Center" />
                                <CellStyle HorizontalAlign="Center">
                                </CellStyle>
                                <PropertiesTextEdit DisplayFormatString="&lt;img src='../imagens/botoes/Demanda{0}.PNG' /&gt;">
                                </PropertiesTextEdit>
                            </dxwtl:TreeListTextColumn>
                        </Columns>
                        <Settings VerticalScrollBarMode="Visible" />
                        <Styles>
                            <Cell Wrap="True">
                            </Cell>
                        </Styles>
                    </dxwtl:aspxtreelist>
            </td>
        </tr>
        <tr>
            <td style="width: 5px"></td>
            <td>
<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tr>
<td>
                <table class="<%=estiloFooter %>" cellspacing="0" cellpadding="0" width="100%"><TBODY><tr><td style="WIDTH: 25px; height: 25px;" valign="middle" align=left><dxe:ASPxImage runat="server" ImageUrl="~/imagens/botoes/DemandaSLenda.png" ID="ASPxImage2"></dxe:ASPxImage>
 </td><td style="WIDTH: 115px; height: 25px;" valign="middle" align=left><dxe:ASPxLabel runat="server" Text="Demanda Simples"  ID="ASPxLabel1"></dxe:ASPxLabel>
 </td><td style="WIDTH: 25px; height: 25px;" valign="middle" align=left><dxe:ASPxImage runat="server" ImageUrl="~/imagens/botoes/DemandaCLenda.png" ID="ASPxImage3"></dxe:ASPxImage>
 </td><td style="WIDTH: 125px; height: 25px;" valign="middle" align=left><dxe:ASPxLabel runat="server" Text="Demanda Complexa"  ID="ASPxLabel2"></dxe:ASPxLabel>
 </td><td valign="middle" style="height: 25px"><span><table style="WIDTH: 100%" cellspacing="0" cellpadding="0"><TBODY><tr><td style="WIDTH: 10px; HEIGHT: 13px; BACKGROUND-COLOR: #ff8000">&nbsp;</td><td style="HEIGHT: 13px">&nbsp;<asp:Label runat="server" Text="Filtros Aplicados"  ID="lblFiltrosAplicados" EnableViewState="False"></asp:Label>
 <span></SPAN></td></tr></tbody></table></SPAN></td></tr></tbody></table>
                            <dxcb:ASPxCallback ID="callback" runat="server" ClientInstanceName="callback" OnCallback="callback_Callback">
                                <ClientSideEvents EndCallback="function(s, e) 
{
	if(s.cp_URL == &quot;X&quot;)
	{
		window.top.mostraMensagem(&quot;O fluxo de nova demanda complexa não foi configurado!&quot;, 'Atencao', true, false, null);
	}else
	{
		window.top.gotoURL(s.cp_URL, '_self');
	}
}" />
                            </dxcb:ASPxCallback>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>

</asp:Content>
