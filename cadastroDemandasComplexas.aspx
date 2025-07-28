<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cadastroDemandasComplexas.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_cadastroDemandasComplexas" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <base target="_self" />
    <title>Demanda Complexa</title>

    <script type="text/javascript" language="javascript">
        function verificaGravacaoInstanciaWf() {
            try {
                // se a tela estiver dentro de um fluxo
                if (null != window.parent.parent.hfGeralWorkflow) {
                    var codigoInstanciaWf = window.parent.parent.hfGeralWorkflow.Get('CodigoInstanciaWf');

                    // se a instância ainda não estiver registrada;
                    if (-1 == codigoInstanciaWf) {
                        window.parent.executaCallbackWF();
                    }
                }
            } catch (e) { }
        }

        function eventoPosSalvar(codigoInstancia) {
            try {
                window.parent.parent.hfGeralWorkflow.Set('CodigoInstanciaWf', codigoInstancia);
            } catch (e) {
            }
        }
    </script>

</head>
<body style="margin-left: 0px; margin-top: 0px" onload='inicializaTela();'>
    <form id="form1" runat="server">

    <table cellpadding="0" cellspacing="0" width="98%">
        <tr>
            <td style="padding-right: 5px; padding-left: 5px; padding-top: 5px">
                <dxtc:ASPxPageControl ID="tcDemanda" runat="server" ActiveTabIndex="0"
                    Width="100%" ClientInstanceName="tcDemanda">
<ContentStyle>
<Paddings Padding="5px"></Paddings>
</ContentStyle>
<TabPages>
<dxtc:TabPage Name="tabPrincipal" Text="Principal"><ContentCollection>
<dxw:ContentControl runat="server"><table cellspacing="0" cellpadding="0" width="100%"><tbody><tr><td><dxe:ASPxLabel runat="server" Text="T&#237;tulo:"  ID="ASPxLabel1d" ></dxe:ASPxLabel>
 </td></tr><tr><td><dxe:ASPxTextBox runat="server" Width="99%" ClientInstanceName="txtTitulo"  ID="txtTitulo" >
<ClientSideEvents ValueChanged="function(s, e) {
	onValueChange_Objects(s, e);
}"></ClientSideEvents>

<ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
<RequiredField IsRequired="True" ErrorText="Campo Obrigat&#243;rio!"></RequiredField>
</ValidationSettings>
</dxe:ASPxTextBox>
 </td></tr><tr><td style="HEIGHT: 10px"></td></tr><tr><td><dxe:ASPxLabel runat="server" Text="Detalhes:"  ID="ASPxLabel2" ></dxe:ASPxLabel>
 </td></tr><tr><td><dxe:ASPxMemo runat="server" Rows="6" Width="99%" ClientInstanceName="txtDetalhes"  TabIndex="1" ID="txtDetalhes" >
<ClientSideEvents ValueChanged="function(s, e) {
	onValueChange_Objects(s, e);
}"></ClientSideEvents>
</dxe:ASPxMemo>
 </td></tr><tr><td style="HEIGHT: 10px"></td></tr><tr><td><table cellspacing="0" cellpadding="0"><tbody><tr><td style="WIDTH: 135px"><dxe:ASPxLabel runat="server" Text="In&#237;cio Previsto:"  ID="ASPxLabel3" ></dxe:ASPxLabel>
 </td><td style="WIDTH: 135px"><dxe:ASPxLabel runat="server" Text="T&#233;rmino Previsto:"  ID="ASPxLabel4" ></dxe:ASPxLabel>
 </td><td style="WIDTH: 78px"><dxe:ASPxLabel runat="server" Text="Prioridade:"  ID="ASPxLabel5" ></dxe:ASPxLabel>
 </td><td style="WIDTH: 270px"><dxe:ASPxLabel runat="server" Text="Demandante:"  ID="ASPxLabel6" ></dxe:ASPxLabel>
 </td><td style="WIDTH: 270px"><dxe:ASPxLabel runat="server" Text="Respons&#225;vel:"  ID="ASPxLabel8" ></dxe:ASPxLabel>
 </td></tr><tr><td style="PADDING-RIGHT: 10px"><dxe:ASPxDateEdit runat="server" Width="100%" DisplayFormatString="dd/MM/yyyy" ClientInstanceName="txtInicio"  TabIndex="2" ID="txtInicio" >
<CalendarProperties TodayButtonText="Hoje" ShowClearButton="False">

 

<DayHeaderStyle ></DayHeaderStyle>

<weeknumberstyle ></weeknumberstyle>

<DayStyle ></DayStyle>

<dayselectedstyle ></dayselectedstyle>

<dayothermonthstyle ></dayothermonthstyle>

<dayweekendstyle ></dayweekendstyle>

<dayoutofrangestyle ></dayoutofrangestyle>

<todaystyle ></todaystyle>

<buttonstyle ></buttonstyle>

<HeaderStyle ></HeaderStyle>

<FooterStyle ></FooterStyle>

<style ></style>
</CalendarProperties>

<ClientSideEvents DateChanged="function(s, e) {
	txtTermino.SetMinDate(s.GetValue());
}" ValueChanged="function(s, e) {
	onValueChange_Objects(s, e);
}"></ClientSideEvents>

<ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
<RequiredField IsRequired="True" ErrorText="Campo Obrigat&#243;rio!"></RequiredField>
</ValidationSettings>
</dxe:ASPxDateEdit>
 </td><td style="PADDING-RIGHT: 10px"><dxe:ASPxDateEdit runat="server" Width="100%" DisplayFormatString="dd/MM/yyyy" ClientInstanceName="txtTermino"  TabIndex="3" ID="txtTermino" >
<CalendarProperties TodayButtonText="Hoje" ShowClearButton="False">

 

<DayHeaderStyle ></DayHeaderStyle>

<weeknumberstyle ></weeknumberstyle>

<DayStyle ></DayStyle>

<dayselectedstyle ></dayselectedstyle>

<dayothermonthstyle ></dayothermonthstyle>

<dayweekendstyle ></dayweekendstyle>

<dayoutofrangestyle ></dayoutofrangestyle>

<todaystyle ></todaystyle>

<buttonstyle ></buttonstyle>

<HeaderStyle ></HeaderStyle>

<FooterStyle ></FooterStyle>

<fastnavstyle ></fastnavstyle>

<fastnavmonthareastyle ></fastnavmonthareastyle>

<fastnavyearareastyle ></fastnavyearareastyle>

<fastnavmonthstyle ></fastnavmonthstyle>

<fastnavyearstyle ></fastnavyearstyle>

<fastnavfooterstyle ></fastnavfooterstyle>

<style ></style>
</CalendarProperties>

<ClientSideEvents ValueChanged="function(s, e) {
	onValueChange_Objects(s, e);
}"></ClientSideEvents>

<ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
<RequiredField IsRequired="True" ErrorText="Campo Obrigat&#243;rio!"></RequiredField>
</ValidationSettings>
</dxe:ASPxDateEdit>
 </td><td style="PADDING-RIGHT: 10px"><dxe:ASPxComboBox runat="server" SelectedIndex="0" ValueType="System.String" Width="100%"  TabIndex="4" ID="ddlPrioridade" ClientInstanceName="ddlPrioridade" >
<ClientSideEvents ValueChanged="function(s, e) {
	onValueChange_Objects(s, e);
}"></ClientSideEvents>
<Items>
<dxe:ListEditItem Text="Alta" Value="A" Selected="True"></dxe:ListEditItem>
<dxe:ListEditItem Text="M&#233;dia" Value="M"></dxe:ListEditItem>
<dxe:ListEditItem Text="Baixa" Value="B"></dxe:ListEditItem>
</Items>
</dxe:ASPxComboBox>
 </td><td style="PADDING-RIGHT: 10px">
            <dxtv:ASPxComboBox ID="ddlDemandante" runat="server" ClientInstanceName="ddlDemandante"  OnItemRequestedByValue="ddlDemandante_ItemRequestedByValue" OnItemsRequestedByFilterCondition="ddlDemandante_ItemsRequestedByFilterCondition" TabIndex="4" Width="100%" 
                IncrementalFilteringMode="Contains"
DropDownStyle="DropDown" 
 EnableCallbackMode="True"
  CallbackPageSize="80" DropDownRows="10">
                <ClientSideEvents ValueChanged="function(s, e) {
	onValueChange_Objects(s, e);
}" />
                <Columns>
                    <dxtv:ListBoxColumn Caption="Nome" ToolTip="Nome do Usuário" />
                    <dxtv:ListBoxColumn Caption="Email" ToolTip="Email do Usuário" />
                </Columns>
            </dxtv:ASPxComboBox>
 </td><td>
            <dxtv:ASPxComboBox ID="ddlResponsavel" runat="server" ClientInstanceName="ddlResponsavel"  OnItemRequestedByValue="ddlResponsavel_ItemRequestedByValue" OnItemsRequestedByFilterCondition="ddlResponsavel_ItemsRequestedByFilterCondition" TabIndex="4" Width="100%"
                IncrementalFilteringMode="Contains"
DropDownStyle="DropDown" 
 EnableCallbackMode="True"
  CallbackPageSize="80" DropDownRows="10">
                <ClientSideEvents ValueChanged="function(s, e) {
	onValueChange_Objects(s, e);
}" />
                <Columns>
                    <dxtv:ListBoxColumn Caption="Nome" ToolTip="Nome do Usuário" />
                    <dxtv:ListBoxColumn Caption="Email" ToolTip="Email do usuário" />
                </Columns>
            </dxtv:ASPxComboBox>
 </td></tr></tbody></table></td></tr><tr><td style="HEIGHT: 10px"></td></tr><tr><td><dxe:ASPxLabel runat="server" Text="Unidade Respons&#225;vel:"  ID="ASPxLabel7" ></dxe:ASPxLabel>
 </td></tr><tr><td><dxe:ASPxComboBox runat="server" ValueType="System.String" Width="555px"  TabIndex="7" ID="ddlUnidade" ClientInstanceName="ddlUnidade" >
<ClientSideEvents ValueChanged="function(s, e) {
	onValueChange_Objects(s, e);
}"></ClientSideEvents>

<ValidationSettings ErrorDisplayMode="ImageWithTooltip" ValidationGroup="MKE">
<RequiredField IsRequired="True" ErrorText="Campo Obrigat&#243;rio!"></RequiredField>
</ValidationSettings>
</dxe:ASPxComboBox>
 </td></tr></tbody></table></dxw:ContentControl>
</ContentCollection>
</dxtc:TabPage>
<dxtc:TabPage ClientVisible="False" Name="tabTarefas" Text="Tarefas"><ContentCollection>
<dxw:ContentControl runat="server"></dxw:ContentControl>
</ContentCollection>
</dxtc:TabPage>
</TabPages>
</dxtc:ASPxPageControl>
            </td>
        </tr>
        <tr>
            <td style="padding-left: 5px; padding-top: 5px">
                            <dxe:ASPxButton ID="btnSalvar" runat="server"
                                Text="Salvar" Width="110px" clientinstancename="btnSalvar" tabindex="8" validationgroup="MKE">
<Paddings Padding="0px"></Paddings>

<ClientSideEvents Click="function(s, e) {
     e.processOnServer = false;	
     var msgCamposValidos = validaCamposFormulario();
    if(trim(msgCamposValidos) == &quot;&quot;)
    {
         existeConteudoCampoAlterado = false;	
        callbackSalvar.PerformCallback();
    }
    else
    {
        window.top.mostraMensagem(msgCamposValidos, 'atencao', true, false, null);
    }
}" ></ClientSideEvents>
</dxe:ASPxButton>
                <dxhf:aspxhiddenfield id="hfGeral" runat="server" clientinstancename="hfGeral"></dxhf:aspxhiddenfield>
                <dxcb:aspxcallback id="callbackSalvar" runat="server" clientinstancename="callbackSalvar" 
                                oncallback="callbackSalvar_Callback">
<ClientSideEvents EndCallback="function(s, e) 
{
	if(s.cp_NovoCodigoDemanda != &quot;&quot;)
	{
		hfGeral.Set(&quot;NovoCodigoDemanda&quot;, s.cp_NovoCodigoDemanda);
	}
	if(s.cp_status == 'ok')
            window.top.mostraMensagem(s.cp_MsgStatus, 'sucesso', false, false, null);
        else
            window.top.mostraMensagem(s.cp_MsgStatus, 'erro', true, false, null);
}" CallbackComplete="function(s, e) {
	verificaGravacaoInstanciaWf();
}" />
</dxcb:aspxcallback>
    <asp:SqlDataSource ID="dsResponsavel" runat="server" ConnectionString="" SelectCommand="">
    </asp:SqlDataSource>
            </td>
        </tr>
    </table>

    </form>
</body>
</html>
