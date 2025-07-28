<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cadastroDemandasSimples.aspx.cs"
    Inherits="BriskPtf._Projetos._Projetos_cadastroDemandasSimples" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <base target="_self" />
    <title>Demanda Simples</title>
    <script type="text/javascript" src="../scripts/PropostaResumo.js">        function framePermissoes_onclick() {

        }

    </script>
    <script type="text/javascript">
        var myObject = null;
        var posExecutar = null;
        var urlModal = "";
        var retornoModal = null;

        function showModal(sUrl, sHeaderTitulo, sWidth, sHeight, sFuncaoPosModal, objParam) {
            if (parseInt(sHeight) < 535)
                sHeight = parseInt(sHeight) + 20;

            myObject = objParam;
            posExecutar = sFuncaoPosModal != "" ? sFuncaoPosModal : null;
            objFrmModal = document.getElementById('frmModal');

            pcModal.SetWidth(sWidth);
            objFrmModal.style.width = "100%";
            objFrmModal.style.height = sHeight + "px";
            urlModal = sUrl;
            //setTimeout ('alteraUrlModal();', 0);            
            pcModal.SetHeaderText(sHeaderTitulo);
            pcModal.Show();

        }

        function fechaModal() {
            pcModal.Hide();
        }

        function resetaModal() {
            objFrmModal = document.getElementById('frmModal');
            posExecutar = null;
            objFrmModal.src = "";
            pcModal.SetHeaderText("");
            retornoModal = null;
        }

    </script>
    <style type="text/css">
        .dxWeb_pcCloseButton
        {
            background-position: 0px -50px;
            width: 15px;
            height: 14px;
        }
        
        .dxWeb_rpHeaderTopLeftCorner, .dxWeb_rpHeaderTopRightCorner, .dxWeb_rpBottomLeftCorner, .dxWeb_rpBottomRightCorner, .dxWeb_rpTopLeftCorner, .dxWeb_rpTopRightCorner, .dxWeb_rpGroupBoxBottomLeftCorner, .dxWeb_rpGroupBoxBottomRightCorner, .dxWeb_rpGroupBoxTopLeftCorner, .dxWeb_rpGroupBoxTopRightCorner, .dxWeb_mHorizontalPopOut, .dxWeb_mVerticalPopOut, .dxWeb_mSubMenuItem, .dxWeb_mSubMenuItemChecked, .dxWeb_nbCollapse, .dxWeb_nbExpand, .dxWeb_splVSeparator, .dxWeb_splVSeparatorHover, .dxWeb_splHSeparator, .dxWeb_splHSeparatorHover, .dxWeb_splVCollapseBackwardButton, .dxWeb_splVCollapseBackwardButtonHover, .dxWeb_splHCollapseBackwardButton, .dxWeb_splHCollapseBackwardButtonHover, .dxWeb_splVCollapseForwardButton, .dxWeb_splVCollapseForwardButtonHover, .dxWeb_splHCollapseForwardButton, .dxWeb_splHCollapseForwardButtonHover, .dxWeb_pcCloseButton, .dxWeb_pcSizeGrip, .dxWeb_pAll, .dxWeb_pAllDisabled, .dxWeb_pPrev, .dxWeb_pPrevDisabled, .dxWeb_pNext, .dxWeb_pNextDisabled, .dxWeb_pLast, .dxWeb_pLastDisabled, .dxWeb_pFirst</style>
</head>
<body style="margin-left: 0px; margin-top: 0px" onload="inicializaTela();">
    <form id="form1" runat="server">
    <div style="padding-top: 3px; padding-left: 0px;">
        <!-- Painel arrendondado lista de projetos -->
        <table>
            <tr>
                <td>
                </td>
                <td>
                    <dxtc:ASPxPageControl ID="tcDemanda" runat="server" ActiveTabIndex="0"
                        Width="100%" ClientInstanceName="tcDemanda">
                        <TabPages>
                            <dxtc:TabPage Name="tabPrincipal" Text="Principal">
                                <ContentCollection>
                                    <dxw:ContentControl runat="server">
                                        <table cellspacing="0" cellpadding="0">
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxLabel runat="server" Text="T&#237;tulo:" 
                                                            ID="ASPxLabel1d">
                                                        </dxe:ASPxLabel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxTextBox runat="server" Width="99%" 
                                                            ID="txtTitulo" MaxLength="255">
                                                            <ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
                                                                <RequiredField IsRequired="True" ErrorText="Campo Obrigatorio!"></RequiredField>
                                                            </ValidationSettings>
                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                            </DisabledStyle>
                                                        </dxe:ASPxTextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 10px">
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxLabel runat="server" Text="Detalhes:" 
                                                            ID="ASPxLabel2">
                                                        </dxe:ASPxLabel>
                                                        &nbsp;
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxMemo runat="server" Rows="10" Width="99%" 
                                                            ID="txtDetalhes">
                                                            <ValidationSettings>
                                                                <RequiredField ErrorText="Campo Obrigatorio!"></RequiredField>
                                                            </ValidationSettings>
                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                            </DisabledStyle>
                                                        </dxe:ASPxMemo>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 10px">
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <table style="width: 99%" cellspacing="0" cellpadding="0">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="width: 115px">
                                                                        <dxe:ASPxLabel runat="server" Text="In&#237;cio Previsto:" 
                                                                            ID="ASPxLabel3">
                                                                        </dxe:ASPxLabel>
                                                                    </td>
                                                                    <td style="width: 115px">
                                                                        <dxe:ASPxLabel runat="server" Text="T&#233;rmino Previsto:"
                                                                            ID="ASPxLabel4">
                                                                        </dxe:ASPxLabel>
                                                                    </td>
                                                                    <td>
                                                                        <dxe:ASPxLabel runat="server" Text="Prioridade:" 
                                                                            ID="ASPxLabel5">
                                                                        </dxe:ASPxLabel>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td style="width: 115px">
                                                                        <dxe:ASPxDateEdit runat="server" Width="110px" DisplayFormatString="dd/MM/yyyy" ClientInstanceName="txtInicio"
                                                                             ID="txtInicio">
                                                                            <CalendarProperties TodayButtonText="Hoje" ShowClearButton="False">
                                                                                <DayHeaderStyle ></DayHeaderStyle>
                                                                                <WeekNumberStyle >
                                                                                </WeekNumberStyle>
                                                                                <DayStyle ></DayStyle>
                                                                                <DaySelectedStyle >
                                                                                </DaySelectedStyle>
                                                                                <DayOtherMonthStyle >
                                                                                </DayOtherMonthStyle>
                                                                                <DayWeekendStyle >
                                                                                </DayWeekendStyle>
                                                                                <DayOutOfRangeStyle >
                                                                                </DayOutOfRangeStyle>
                                                                                <TodayStyle >
                                                                                </TodayStyle>
                                                                                <ButtonStyle >
                                                                                </ButtonStyle>
                                                                                <HeaderStyle ></HeaderStyle>
                                                                                <FooterStyle ></FooterStyle>
                                                                                <FastNavMonthAreaStyle >
                                                                                </FastNavMonthAreaStyle>
                                                                                <FastNavMonthStyle >
                                                                                </FastNavMonthStyle>
                                                                                <FastNavFooterStyle >
                                                                                </FastNavFooterStyle>
                                                                                <Style ></Style>
                                                                            </CalendarProperties>
                                                                            <ClientSideEvents DateChanged="function(s, e) {
	txtTermino.SetMinDate(s.GetValue());
}"></ClientSideEvents>
                                                                            <ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
                                                                                <RequiredField IsRequired="True" ErrorText="Campo Obrigatorio!"></RequiredField>
                                                                            </ValidationSettings>
                                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                                            </DisabledStyle>
                                                                        </dxe:ASPxDateEdit>
                                                                    </td>
                                                                    <td style="width: 115px">
                                                                        <dxe:ASPxDateEdit runat="server" Width="110px" DisplayFormatString="dd/MM/yyyy" ClientInstanceName="txtTermino"
                                                                             ID="txtTermino">
                                                                            <CalendarProperties TodayButtonText="Hoje" ShowClearButton="False">
                                                                                <DayHeaderStyle ></DayHeaderStyle>
                                                                                <WeekNumberStyle >
                                                                                </WeekNumberStyle>
                                                                                <DayStyle ></DayStyle>
                                                                                <DaySelectedStyle >
                                                                                </DaySelectedStyle>
                                                                                <DayOtherMonthStyle >
                                                                                </DayOtherMonthStyle>
                                                                                <DayWeekendStyle >
                                                                                </DayWeekendStyle>
                                                                                <DayOutOfRangeStyle >
                                                                                </DayOutOfRangeStyle>
                                                                                <TodayStyle >
                                                                                </TodayStyle>
                                                                                <ButtonStyle >
                                                                                </ButtonStyle>
                                                                                <HeaderStyle ></HeaderStyle>
                                                                                <FooterStyle ></FooterStyle>
                                                                                <FastNavMonthAreaStyle >
                                                                                </FastNavMonthAreaStyle>
                                                                                <FastNavMonthStyle >
                                                                                </FastNavMonthStyle>
                                                                                <FastNavFooterStyle >
                                                                                </FastNavFooterStyle>
                                                                                <Style ></Style>
                                                                            </CalendarProperties>
                                                                            <ClientSideEvents DateChanged="function(s, e) {
	txtTermino.SetMinDate(s.GetValue());
}" />
                                                                            <ClientSideEvents DateChanged="function(s, e) {
	txtTermino.SetMinDate(s.GetValue());
}"></ClientSideEvents>
                                                                            <ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
                                                                                <RequiredField IsRequired="True" ErrorText="Campo Obrigatorio!"></RequiredField>
                                                                            </ValidationSettings>
                                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                                            </DisabledStyle>
                                                                        </dxe:ASPxDateEdit>
                                                                    </td>
                                                                    <td>
                                                                        <dxe:ASPxComboBox runat="server" SelectedIndex="0" ValueType="System.String" Width="68px"
                                                                             ID="ddlPrioridade">
                                                                            <Items>
                                                                                <dxe:ListEditItem Text="Alta" Value="A" Selected="True"></dxe:ListEditItem>
                                                                                <dxe:ListEditItem Text="M&#233;dia" Value="M"></dxe:ListEditItem>
                                                                                <dxe:ListEditItem Text="Baixa" Value="B"></dxe:ListEditItem>
                                                                            </Items>
                                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                                            </DisabledStyle>
                                                                        </dxe:ASPxComboBox>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 10px">
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxLabel runat="server" Text="Demandante:" 
                                                            ID="ASPxLabel6">
                                                        </dxe:ASPxLabel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxComboBox ID="ddlDemandante" runat="server" ClientInstanceName="ddlDemandante"
                                                             IncrementalFilteringMode="Contains" ValueType="System.String"
                                                            Width="555px" TextField="ColunaNome" ValueField="ColunaValor" DropDownStyle="DropDown"
                                                            EnableCallbackMode="True" OnItemRequestedByValue="ddlDemandante_ItemRequestedByValue"
                                                            OnItemsRequestedByFilterCondition="ddlDemandante_ItemsRequestedByFilterCondition">
                                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	hfGeral.Set(&quot;CodigoDemandante&quot;,s.GetValue());
}"></ClientSideEvents>
                                                            <Columns>
                                                                <dxe:ListBoxColumn Caption="Nome" Width="250px" />
                                                                <dxe:ListBoxColumn Caption="Email" Width="300px" />
                                                            </Columns>
                                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	hfGeral.Set(&quot;CodigoDemandante&quot;,s.GetValue());
}" />
                                                            <ValidationSettings CausesValidation="True" Display="Dynamic" ErrorDisplayMode="ImageWithTooltip"
                                                                ValidationGroup="MKE">
                                                                <RequiredField ErrorText="" IsRequired="True" />
                                                                <RequiredField IsRequired="True" ErrorText=""></RequiredField>
                                                            </ValidationSettings>
                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                            </DisabledStyle>
                                                        </dxe:ASPxComboBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 10px">
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxLabel runat="server" Text="Respons&#225;vel:" 
                                                            ID="ASPxLabel8">
                                                        </dxe:ASPxLabel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxComboBox ID="ddlResponsavel" runat="server" ClientInstanceName="ddlResponsavel"
                                                             IncrementalFilteringMode="Contains" ValueType="System.String"
                                                            Width="555px" TextField="ColunaNome" ValueField="ColunaValor" DropDownStyle="DropDown"
                                                            EnableCallbackMode="True" OnItemRequestedByValue="ddlResponsavel_ItemRequestedByValue"
                                                            OnItemsRequestedByFilterCondition="ddlResponsavel_ItemsRequestedByFilterCondition">
                                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	hfGeral.Set(&quot;CodigoResponsavel&quot;,s.GetValue());
}"></ClientSideEvents>
                                                            <Columns>
                                                                <dxe:ListBoxColumn Caption="Nome" Width="250px" />
                                                                <dxe:ListBoxColumn Caption="Email" Width="300px" />
                                                            </Columns>
                                                            <ClientSideEvents SelectedIndexChanged="function(s, e) {
	hfGeral.Set(&quot;CodigoResponsavel&quot;,s.GetValue());
}" />
                                                            <ValidationSettings CausesValidation="True" Display="Dynamic" ErrorDisplayMode="ImageWithTooltip"
                                                                ValidationGroup="MKE">
                                                                <RequiredField ErrorText="" IsRequired="True" />
                                                                <RequiredField IsRequired="True" ErrorText=""></RequiredField>
                                                            </ValidationSettings>
                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                            </DisabledStyle>
                                                        </dxe:ASPxComboBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 10px">
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxLabel runat="server" Text="Unidade Respons&#225;vel:"
                                                            ID="lblUnidadeResponsavel" ClientInstanceName="lblUnidadeResponsavel">
                                                        </dxe:ASPxLabel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <dxe:ASPxComboBox runat="server" ValueType="System.String" Width="555px"
                                                            ID="ddlUnidade" IncrementalFilteringMode="Contains" TextField="NomeUnidadeNegocio"
                                                            TextFormatString="{0}" ValueField="CodigoUnidadeNegocio">
                                                            <ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
                                                                <RequiredField IsRequired="True" ErrorText="Campo Obrigatorio!"></RequiredField>
                                                            </ValidationSettings>
                                                            <Columns>
                                                                <dxe:ListBoxColumn Caption="Sigla" FieldName="SiglaUnidadeNegocio" Width="100px" />
                                                                <dxe:ListBoxColumn Caption="Unidade" FieldName="NomeUnidadeNegocio" Width="300px" />
                                                            </Columns>
                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                            </DisabledStyle>
                                                        </dxe:ASPxComboBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 69px">
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <dxhf:ASPxHiddenField runat="server" ClientInstanceName="hfGeral" ID="hfGeral">
                                        </dxhf:ASPxHiddenField>
                                        <dxcb:ASPxCallback runat="server" ClientInstanceName="callbackDemandante" ID="callbackDemandante"
                                            OnCallback="callbackDemandante_Callback">
                                            <ClientSideEvents EndCallback="function(s, e) 
{
	if(s.cp_Codigo != &quot;&quot;)
	{
		hfGeral.Set(&quot;CodigoDemandante&quot;, s.cp_Codigo);
		txtDemandante.SetText(s.cp_Nome);
	}
	else
	{
		mostraLovDemandante(s.cp_Where);
	}
}"></ClientSideEvents>
                                        </dxcb:ASPxCallback>
                                        <dxcb:ASPxCallback runat="server" ClientInstanceName="callbackResponsavel" ID="callbackResponsavel"
                                            OnCallback="callbackResponsavel_Callback">
                                            <ClientSideEvents EndCallback="function(s, e) 
{
	if(s.cp_Codigo != &quot;&quot;)
	{
		hfGeral.Set(&quot;CodigoResponsavel&quot;, s.cp_Codigo);
		txtResponsavel.SetText(s.cp_Nome);
	}
	else
	{
		mostraLovResponsavel(s.cp_Where);
	}
}"></ClientSideEvents>
                                        </dxcb:ASPxCallback>
                                        <dxcb:ASPxCallback runat="server" ClientInstanceName="callbackSalvar" ID="callbackSalvar"
                                            OnCallback="callbackSalvar_Callback">
                                        </dxcb:ASPxCallback>
                                        <asp:SqlDataSource ID="dsResponsavel" runat="server"></asp:SqlDataSource>
                                        <asp:SqlDataSource ID="dsResponsavel0" runat="server"></asp:SqlDataSource>
                                    </dxw:ContentControl>
                                </ContentCollection>
                            </dxtc:TabPage>
                            <dxtc:TabPage Name="tabTarefas" Text="Tarefas">
                                <ContentCollection>
                                    <dxw:ContentControl runat="server">
                                    </dxw:ContentControl>
                                </ContentCollection>
                            </dxtc:TabPage>
                            <dxtc:TabPage Name="tabEncerramento" Text="Status da Demanda">
                                <ContentCollection>
                                    <dxw:ContentControl runat="server">
                                        <table>
                                            <tr>
                                                <td>
                                                    <dxe:ASPxLabel runat="server" Text="Status da Demanda:" 
                                                        ID="ASPxLabel9s">
                                                    </dxe:ASPxLabel>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <dxe:ASPxComboBox runat="server" ValueType="System.String" Width="310px"
                                                        ID="ddlStatus">
                                                        <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                        </DisabledStyle>
                                                    </dxe:ASPxComboBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="height: 10px">
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <dxe:ASPxLabel runat="server" Text="Coment&#225;rios:" 
                                                        ID="ASPxLabel10">
                                                    </dxe:ASPxLabel>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <dxhe:ASPxHtmlEditor runat="server" Width="100%" Height="399px"
                                                        ID="txtComentarios">
                                                        <StylesToolbars>
                                                            <Toolbar >
                                                            </Toolbar>
                                                            <ToolbarItem >
                                                            </ToolbarItem>
                                                        </StylesToolbars>
                                                        <StylesContextMenu>
                                                            <Item >
                                                            </Item>
                                                            <Style ></Style>
                                                        </StylesContextMenu>
                                                        <Settings AllowHtmlView="False" AllowPreview="False"></Settings>
                                                        <Toolbars>
                                                            <dxhe:HtmlEditorToolbar>
                                                                <Items>
                                                                    <dxhe:ToolbarCutButton>
                                                                    </dxhe:ToolbarCutButton>
                                                                    <dxhe:ToolbarCopyButton>
                                                                    </dxhe:ToolbarCopyButton>
                                                                    <dxhe:ToolbarPasteButton>
                                                                    </dxhe:ToolbarPasteButton>
                                                                    <dxhe:ToolbarPasteFromWordButton>
                                                                    </dxhe:ToolbarPasteFromWordButton>
                                                                    <dxhe:ToolbarUndoButton BeginGroup="True">
                                                                    </dxhe:ToolbarUndoButton>
                                                                    <dxhe:ToolbarRedoButton>
                                                                    </dxhe:ToolbarRedoButton>
                                                                    <dxhe:ToolbarRemoveFormatButton BeginGroup="True">
                                                                    </dxhe:ToolbarRemoveFormatButton>
                                                                    <dxhe:ToolbarSuperscriptButton BeginGroup="True">
                                                                    </dxhe:ToolbarSuperscriptButton>
                                                                    <dxhe:ToolbarSubscriptButton>
                                                                    </dxhe:ToolbarSubscriptButton>
                                                                    <dxhe:ToolbarInsertOrderedListButton BeginGroup="True">
                                                                    </dxhe:ToolbarInsertOrderedListButton>
                                                                    <dxhe:ToolbarInsertUnorderedListButton>
                                                                    </dxhe:ToolbarInsertUnorderedListButton>
                                                                    <dxhe:ToolbarIndentButton BeginGroup="True">
                                                                    </dxhe:ToolbarIndentButton>
                                                                    <dxhe:ToolbarOutdentButton>
                                                                    </dxhe:ToolbarOutdentButton>
                                                                    <dxhe:ToolbarInsertLinkDialogButton BeginGroup="True">
                                                                    </dxhe:ToolbarInsertLinkDialogButton>
                                                                    <dxhe:ToolbarUnlinkButton>
                                                                    </dxhe:ToolbarUnlinkButton>
                                                                    <dxhe:ToolbarInsertImageDialogButton>
                                                                    </dxhe:ToolbarInsertImageDialogButton>
                                                                    <dxhe:ToolbarCheckSpellingButton BeginGroup="True" Visible="False">
                                                                    </dxhe:ToolbarCheckSpellingButton>
                                                                    <dxhe:ToolbarTableOperationsDropDownButton BeginGroup="True">
                                                                        <Items>
                                                                            <dxhe:ToolbarInsertTableDialogButton BeginGroup="True" ViewStyle="ImageAndText">
                                                                            </dxhe:ToolbarInsertTableDialogButton>
                                                                            <dxhe:ToolbarTablePropertiesDialogButton BeginGroup="True">
                                                                            </dxhe:ToolbarTablePropertiesDialogButton>
                                                                            <dxhe:ToolbarTableRowPropertiesDialogButton>
                                                                            </dxhe:ToolbarTableRowPropertiesDialogButton>
                                                                            <dxhe:ToolbarTableColumnPropertiesDialogButton>
                                                                            </dxhe:ToolbarTableColumnPropertiesDialogButton>
                                                                            <dxhe:ToolbarTableCellPropertiesDialogButton>
                                                                            </dxhe:ToolbarTableCellPropertiesDialogButton>
                                                                            <dxhe:ToolbarInsertTableRowAboveButton BeginGroup="True">
                                                                            </dxhe:ToolbarInsertTableRowAboveButton>
                                                                            <dxhe:ToolbarInsertTableRowBelowButton>
                                                                            </dxhe:ToolbarInsertTableRowBelowButton>
                                                                            <dxhe:ToolbarInsertTableColumnToLeftButton>
                                                                            </dxhe:ToolbarInsertTableColumnToLeftButton>
                                                                            <dxhe:ToolbarInsertTableColumnToRightButton>
                                                                            </dxhe:ToolbarInsertTableColumnToRightButton>
                                                                            <dxhe:ToolbarSplitTableCellHorizontallyButton BeginGroup="True">
                                                                            </dxhe:ToolbarSplitTableCellHorizontallyButton>
                                                                            <dxhe:ToolbarSplitTableCellVerticallyButton>
                                                                            </dxhe:ToolbarSplitTableCellVerticallyButton>
                                                                            <dxhe:ToolbarMergeTableCellRightButton>
                                                                            </dxhe:ToolbarMergeTableCellRightButton>
                                                                            <dxhe:ToolbarMergeTableCellDownButton>
                                                                            </dxhe:ToolbarMergeTableCellDownButton>
                                                                            <dxhe:ToolbarDeleteTableButton BeginGroup="True">
                                                                            </dxhe:ToolbarDeleteTableButton>
                                                                            <dxhe:ToolbarDeleteTableRowButton>
                                                                            </dxhe:ToolbarDeleteTableRowButton>
                                                                            <dxhe:ToolbarDeleteTableColumnButton>
                                                                            </dxhe:ToolbarDeleteTableColumnButton>
                                                                        </Items>
                                                                    </dxhe:ToolbarTableOperationsDropDownButton>
                                                                    <dxhe:ToolbarFullscreenButton>
                                                                    </dxhe:ToolbarFullscreenButton>
                                                                </Items>
                                                            </dxhe:HtmlEditorToolbar>
                                                            <dxhe:HtmlEditorToolbar>
                                                                <Items>
                                                                    <dxhe:ToolbarParagraphFormattingEdit Width="120px">
                                                                        <Items>
                                                                            <dxhe:ToolbarListEditItem Text="Normal" Value="p" />
                                                                            <dxhe:ToolbarListEditItem Text="Heading  1" Value="h1" />
                                                                            <dxhe:ToolbarListEditItem Text="Heading  2" Value="h2" />
                                                                            <dxhe:ToolbarListEditItem Text="Heading  3" Value="h3" />
                                                                            <dxhe:ToolbarListEditItem Text="Heading  4" Value="h4" />
                                                                            <dxhe:ToolbarListEditItem Text="Heading  5" Value="h5" />
                                                                            <dxhe:ToolbarListEditItem Text="Heading  6" Value="h6" />
                                                                            <dxhe:ToolbarListEditItem Text="Address" Value="address" />
                                                                            <dxhe:ToolbarListEditItem Text="Normal (DIV)" Value="div" />
                                                                        </Items>
                                                                    </dxhe:ToolbarParagraphFormattingEdit>
                                                                    <dxhe:ToolbarFontNameEdit>
                                                                        <Items>
                                                                            <dxhe:ToolbarListEditItem Text="Times New Roman" Value="Times New Roman" />
                                                                            <dxhe:ToolbarListEditItem Text="Tahoma" Value="Tahoma" />
                                                                            <dxhe:ToolbarListEditItem Text="Verdana" Value="Verdana" />
                                                                            <dxhe:ToolbarListEditItem Text="Arial" Value="Arial" />
                                                                            <dxhe:ToolbarListEditItem Text="MS Sans Serif" Value="MS Sans Serif" />
                                                                            <dxhe:ToolbarListEditItem Text="Courier" Value="Courier" />
                                                                        </Items>
                                                                    </dxhe:ToolbarFontNameEdit>
                                                                    <dxhe:ToolbarFontSizeEdit>
                                                                        <Items>
                                                                            <dxhe:ToolbarListEditItem Text="1 (8pt)" Value="1" />
                                                                            <dxhe:ToolbarListEditItem Text="2 (10pt)" Value="2" />
                                                                            <dxhe:ToolbarListEditItem Text="3 (12pt)" Value="3" />
                                                                            <dxhe:ToolbarListEditItem Text="4 (14pt)" Value="4" />
                                                                            <dxhe:ToolbarListEditItem Text="5 (18pt)" Value="5" />
                                                                            <dxhe:ToolbarListEditItem Text="6 (24pt)" Value="6" />
                                                                            <dxhe:ToolbarListEditItem Text="7 (36pt)" Value="7" />
                                                                        </Items>
                                                                    </dxhe:ToolbarFontSizeEdit>
                                                                    <dxhe:ToolbarBoldButton BeginGroup="True">
                                                                    </dxhe:ToolbarBoldButton>
                                                                    <dxhe:ToolbarItalicButton>
                                                                    </dxhe:ToolbarItalicButton>
                                                                    <dxhe:ToolbarUnderlineButton>
                                                                    </dxhe:ToolbarUnderlineButton>
                                                                    <dxhe:ToolbarStrikethroughButton>
                                                                    </dxhe:ToolbarStrikethroughButton>
                                                                    <dxhe:ToolbarJustifyLeftButton BeginGroup="True">
                                                                    </dxhe:ToolbarJustifyLeftButton>
                                                                    <dxhe:ToolbarJustifyCenterButton>
                                                                    </dxhe:ToolbarJustifyCenterButton>
                                                                    <dxhe:ToolbarJustifyRightButton>
                                                                    </dxhe:ToolbarJustifyRightButton>
                                                                    <dxhe:ToolbarJustifyFullButton>
                                                                    </dxhe:ToolbarJustifyFullButton>
                                                                    <dxhe:ToolbarBackColorButton BeginGroup="True">
                                                                    </dxhe:ToolbarBackColorButton>
                                                                    <dxhe:ToolbarFontColorButton>
                                                                    </dxhe:ToolbarFontColorButton>
                                                                </Items>
                                                            </dxhe:HtmlEditorToolbar>
                                                        </Toolbars>
                                                    </dxhe:ASPxHtmlEditor>
                                                </td>
                                            </tr>
                                        </table>
                                    </dxw:ContentControl>
                                </ContentCollection>
                            </dxtc:TabPage>
                            <dxtc:TabPage Name="tabPermissoes" Text="Interessados">
                                <ContentCollection>
                                    <dxw:ContentControl runat="server">
                                        <iframe id="framePermissoes" frameborder="0" scrolling="no" style="width: 100%; height: 455px">
                                        </iframe>
                                    </dxw:ContentControl>
                                </ContentCollection>
                            </dxtc:TabPage>
                            <dxtc:TabPage Name="tabMensagens" Text="Mensagens">
                                <ContentCollection>
                                    <dxw:ContentControl runat="server" SupportsDisabledAttribute="True">
                                        <iframe id="frameMsg" frameborder="0" scrolling="no" style="width: 100%; height: 455px">
                                        </iframe>
                                    </dxw:ContentControl>
                                </ContentCollection>
                            </dxtc:TabPage>
                            <dxtc:TabPage Name="tabAnexos" Text="Anexos">
                                <ContentCollection>
                                    <dxw:ContentControl runat="server" SupportsDisabledAttribute="True">
                                        <iframe id="frameAnexos" frameborder="0" scrolling="no" style="width: 100%; height: 455px">
                                        </iframe>
                                    </dxw:ContentControl>
                                </ContentCollection>
                            </dxtc:TabPage>
                        </TabPages>
                        <ClientSideEvents ActiveTabChanged="function(s, e) {                        
	if(e.tab.name == &quot;tabPermissoes&quot; &amp;&amp; document.getElementById('framePermissoes').src == &quot;&quot;)
		document.getElementById('framePermissoes').src = s.cp_URL;
    else if(e.tab.name == &quot;tabMensagens&quot; &amp;&amp; document.getElementById('frameMsg').src == &quot;&quot;)
		document.getElementById('frameMsg').src = s.cp_URLMsg;
    else if(e.tab.name == &quot;tabAnexos&quot; &amp;&amp; document.getElementById('frameAnexos').src == &quot;&quot;)
		document.getElementById('frameAnexos').src = s.cp_URLAnexos;
}" />
                    </dxtc:ASPxPageControl>
                </td>
                <td style="width: 5px">
                </td>
            </tr>
            <tr>
                <td style="width: 10px; height: 10px">
                </td>
                <td style="height: 10px">
                </td>
                <td style="width: 5px; height: 10px">
                </td>
            </tr>
            <tr>
                <td>
                </td>
                <td align="right">
                    <table cellpadding="0" cellspacing="0" style="width: 200px">
                        <tr>
                            <td align="right">
                                <dxe:ASPxButton ID="ASPxButton1" runat="server" AutoPostBack="False"
                                    Text="Salvar" Width="90px" OnClick="ASPxButton1_Click" ValidationGroup="MKE">
                                    <Paddings Padding="0px"></Paddings>
                                    <ClientSideEvents Click="function(s, e) 
{
     callbackSalvar.PerformCallback();
}"></ClientSideEvents>
                                </dxe:ASPxButton>
                            </td>
                            <td align="right">
                                <dxe:ASPxButton ID="btnCancelar" runat="server" AutoPostBack="False"
                                    Text="Fechar" Width="90px">
                                    <Paddings Padding="0px" />
                                    <ClientSideEvents Click="function(s, e) {
	window.top.fechaModal();
}" />
                                </dxe:ASPxButton>
                            </td>
                        </tr>
                    </table>
                </td>
                <td style="width: 5px">
                </td>
            </tr>
        </table>
    </div>
    <dxpc:ASPxPopupControl ID="pcModal" runat="server" ClientInstanceName="pcModal"
        HeaderText="" Modal="True" PopupHorizontalAlign="WindowCenter"
        PopupVerticalAlign="WindowCenter" AllowDragging="True" AllowResize="True" CloseAction="CloseButton">
        <ContentCollection>
            <dxpc:PopupControlContentControl ID="PopupControlContentControl8" runat="server">
                <iframe id="frmModal" name="frmModal" frameborder="0" style="overflow: auto; padding: 0px;
                    margin: 0px;"></iframe>
            </dxpc:PopupControlContentControl>
        </ContentCollection>
        <ClientSideEvents CloseUp="function(s, e) {
            var retorno = '';
            
            if(retornoModal != null)
            {   
                retorno = retornoModal;
            }
            
            if(posExecutar != null)
               posExecutar(retorno);
                
            resetaModal();
}" PopUp="function(s, e) {
    window.document.getElementById('frmModal').dialogArguments = myObject;
	document.getElementById('frmModal').src = urlModal;
}" />
        <ContentStyle>
            <Paddings Padding="5px" />
        </ContentStyle>
    </dxpc:ASPxPopupControl>
    </form>
</body>
</html>
