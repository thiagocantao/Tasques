<%@ Page Language="C#" MasterPageFile="~/novaCdis.master" AutoEventWireup="true" CodeBehind="resumoProcessos.aspx.cs"
    Inherits="BriskPtf._Projetos._Projetos_resumoProcessos" Title="Portal da Estratégia" %>

<%@ MasterType VirtualPath="~/novaCdis.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="AreaTrabalho" EnableViewState="false" runat="Server">
    <div enableviewstate="false">
        <table border="0" cellpadding="0" cellspacing="0" style="width: 100%;>
            <tr height="26px">
                <td valign="middle" style="padding-left: 10px">
                    <asp:Label ID="lblTituloTela" runat="server" Font-Bold="True"
                        Font-Overline="False" Font-Strikeout="False" Text="Processos da Instituição"
                        EnableViewState="False"></asp:Label>
                </td>
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
                        <td align="left" valign="bottom" >
                            <dxe:ASPxImage ID="ASPxImage1" runat="server" Cursor="pointer" ImageUrl="~/imagens/botoes/novoReg.PNG"
                                ToolTip="Incluir Novo Processo">
<ClientSideEvents Click="function(s, e) {
	var url = &quot;./Administracao/cadastroProcessos.aspx&quot;;
    window.top.showModal(url, 'Inclusão de Processo', 840, 480, atualizaGrid, null); 
    
}"></ClientSideEvents>
</dxe:ASPxImage>
                        </td>
                        <td align="left" style="width: 155px">
                            <table>
                                <tr>
                                    <td align="left" style="width: 155px">
                                        <asp:Label ID="Label1" runat="server" EnableViewState="False"
                                            Text="Nome do Processo:"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td align="left" style="width: 155px">
                                        <dxe:ASPxTextBox ID="txtDescricao" runat="server" ClientInstanceName="txtDescricao"
                                            EnableViewState="False"  ToolTip="Informe parte do nome do Projeto para a pesquisa."
                                            Width="150px">
                                            <ClientSideEvents KeyDown="function(s, e) {
                                            novaDescricao();	
                                            }" />
                                        </dxe:ASPxTextBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 255px;" id="td_FiltroGerente" align="left">
                            <table>
                                <tr>
                                    <td align="left" style="width: 255px">
                                        <asp:Label ID="lblGerentes" runat="server"  Text="Responsável:" EnableViewState="False"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td align="left" style="width: 255px">
                                        <dxe:ASPxComboBox ID="ddlGerentes" runat="server" 
                                            ClientInstanceName="ddlGerentes" ValueType="System.String" Width="250px" 
                                             EnableViewState="False">
                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	                                            mudaCorComboDev(ddlGerentes, 'lblGerentes', '-1'); 
	                                            tlProjetos.PerformCallback();
                                            }" Init="function(s, e) {
	                                            mudaCorComboDev(ddlGerentes, 'lblGerentes', '-1'); 
                                            }" />
                                        </dxe:ASPxComboBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 100px;" id="td_FiltroUnidade" align="left">
                            <table>
                                <tr>
                                    <td align="left" style="width: 100px">
                                        <asp:Label ID="lblUnidades" runat="server" EnableViewState="False" 
                                            Text="Unidade:"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td align="left" style="width: 100px">
                                        <dxe:ASPxComboBox ID="ddlUnidades" runat="server" 
                                            ClientInstanceName="ddlUnidades" ValueType="System.String" Width="95px" 
                                             EnableViewState="False" 
                                            style="margin-bottom: 0px">
                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	mudaCorComboDev(ddlUnidades, 'lblUnidades', '-1'); 
	tlProjetos.PerformCallback();
}" Init="function(s, e) {
	mudaCorComboDev(ddlUnidades, 'lblUnidades', '-1'); 
}" />
                                        </dxe:ASPxComboBox>
                                    </td>
                                </tr>
                            </table>
                        </td>                        
                        <td style="width: 220px" align="left">
                            <table>
                                <tr>
                                    <td align="left" style="width: 220px">
                                        <asp:Label ID="lblStatus" runat="server"  Text="Status:"
                                            EnableViewState="False"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="left" width="220px">
                                        <dxe:ASPxComboBox ID="ddlStatus" runat="server" ClientInstanceName="ddlStatus"
                                            Width="215px"  EnableViewState="False">
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
                         keyfieldname="Codigo" 
                        parentfieldname="CodigoProjetoPai" ClientInstanceName="tlProjetos" 
                        OnCustomCallback="tlProjetos_CustomCallback" 
                        OnHtmlDataCellPrepared="tlProjetos_HtmlDataCellPrepared" Width="100%" 
                        EnableViewState="False" >
                        <Columns>
                            <dxwtl:TreeListTextColumn Caption="Descri&#231;&#227;o" FieldName="Descricao" Name="Descricao"
                                VisibleIndex="0">
                                <DataCellTemplate>
                                    <%# (Eval("IndicaProjeto").ToString() == "S") ? "<table><tr><td>&nbsp;<a class='LinkGrid' href='#' onclick='abreResumoProcesso(" + Eval("CodigoProjeto").ToString() + ",\"" + Eval("Descricao").ToString() + "\");'>" + Eval("Descricao") + "</a></td></tr></table>" : "<table><tr><td>" + Eval("Descricao").ToString() + "</td></tr></table>"%>
                                </DataCellTemplate>
                                <HeaderStyle HorizontalAlign="Left" />
                                <CellStyle HorizontalAlign="Left">
                                    <Paddings PaddingLeft="1px" />
                                </CellStyle>
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Respons&#225;vel" FieldName="GerenteProjeto" Name="Gerente"
                                VisibleIndex="1" Width="200px">
                            </dxwtl:TreeListTextColumn>
                            <dxwtl:TreeListTextColumn Caption="Status" FieldName="StatusProjeto" Name="Status"
                                VisibleIndex="2" Width="205px">
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
            <td><table class="<%=estiloFooter %>" cellspacing="0" cellpadding="0" width="100%"><TBODY><tr><td 
style="HEIGHT: 25px" valign="middle"><SPAN 
style="FONT-SIZE: 8pt"><table cellspacing="0" 
cellpadding="0" border="0"><TBODY><tr><td style="WIDTH: 125px" valign="middle"><span><table style="WIDTH: 100%" cellspacing="0" 
cellpadding="0"><TBODY><tr><td 
style="WIDTH: 10px; HEIGHT: 13px; BACKGROUND-COLOR: #ff8000"> &nbsp;&nbsp;</td><td 
style="FONT-SIZE: 8pt; HEIGHT: 13px"> &nbsp;
    <asp:Label id="lblFiltrosAplicados" runat="server" Font-Size="7pt" EnableViewState="False" Text="Filtros Aplicados"></asp:Label> 
<span></SPAN></td></tr></tbody></table></SPAN></td></tr></tbody></table></SPAN></td></tr></tbody></table>
            </td>
        </tr>
    </table>

</asp:Content>
