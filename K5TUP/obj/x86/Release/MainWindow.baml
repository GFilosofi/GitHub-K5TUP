   M S B A M L   `   `   `  ���� 	  K5TUP    K5TUP.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � Title$  
MainWindow��$y�Visible=� � WindowStartupLocation$ CenterScreen=�$��0q� � 
ResizeMode$ NoResize=�
���   � J� OpacityMask
 �   � � WindowStyle$
 None=�$��0q� � Icon$ Images\LOGO.ico�� � SizeToContent$ WidthAndHeight=�$��LeftToRight=�$	��Top=�
���   � J� Opacity$ 10Q� � AllowsTransparency . ��Z� 	 � 	Resources	    ComPorts
 � Source � XPath  ComSpeed  DevNames��%�   &	        &	 ,     &	 W     � $
 CommsData.xml?�$ /Comms/Ports��� $
 CommsData.xml?�$ /Comms/Baud��� $
 DevicesData.xml?�$ 
/Devs/Name��.���� -    �� CornerRadius$ 10��
���   �$��1q�$��0,0,0,0q�$	��830��$	��389������  �� EndPoint$ 0.178,0.939&� � MappingMode$ RelativeToBoundingBox=� �� 
StartPoint$ -0.100,0.209&�.���  � Color$ DarkBlue�� � Offset$ 0.061Q�� $ White��$	 1.1Q�._�� $��0q�$	��830�� � ColumnDefinitions �� $��85 ��� $
��129* ��� $
��154* ��� $
��122* ��� $
��272* ��� $	��68* � � RowDefinitions �� $��22.243 ��� $��18.61 ��� $
��304* �.+��� -   $��Device_Name��$��2��$��20��$
��Left=�$	��Top=�$��0,43,0,0q�$��1��$	��123��$��2�� � ToolTip$ select a device���� -    ��Name$ Comm_Port_Names��$��2��$��20��$
��Left=�$��
0,64,0,262q�$��1��$	��123��$��2��$; 5select the COM port associated to the selected device���� -   $ 
Baud_Rates��$��2��$��20��$
��Left=�$��
0,85,0,241q�$��1��$	��123��$��2��$< 6select the baud-rate associated to the selected device���� -   $ MsgData��$��2��$��20��$
��Left=�$��0,109,0,217q�$��1��$��3��$	��403��$' !select either a command or a test���� -    ��Name$ 
SerialData��$��2��$��20��$��Stretch=� �� TextAlignment$ Center=�$��0,133,2,193q�$��1��$��3��$, &edit/select either a command or a test���� $��2��$��20�� �� TextAlignment$ Center=�$��Device Name��
�������$��Bottom=�$��	0,0,0,282q�$ 1Q��� $��2��$��20��$ Center=�$��Com Port��
�������$��Bottom=�$��	0,0,0,262q�$ 1Q��� $��2��$��20��$ Center=�$��	Baud Rate��
�������$	��Top=�$��	-1,86,1,0q�$ 1Q�$y�Visible=��� $��2��$��20��$ Center=�$��
Select Box��
�������$��Bottom=�$��	0,0,0,216q�$ 1Q��� $��2��$��20��$ Center=�$��Send Box��
�������$��	2,0,0,192q�$��Bottom=��� $��2��$��20��$ Center=�$��Receive Box��
�������$��Bottom=�$��	-2,0,0,97q��� -    ��Name$ 
ReceiveBox��$��2��$��
0,156,0,23q�$
��Left=�$	��402��$
��Auto=�}�.  $��1��$��3���� -   $��Sel_btn��$��Select��$��4��$��2��$
��Left=�$	��Top=�  Send_Btn_Style#���  $��32��$��73��$��7,98,0,0q��� -	   $��Send_btn��$��	Send/Exec��$��4��$��2��$
��Left=�$��Bottom=�#���  $��32��$��73��$��	7,0,0,182q�$0 *send (execute) the selected command (test)���� -
   $��Button��$��5�� J� RenderTransformOrigin$ 1.684,0.257&�$��20��  Close_Btn_Stle#���  $��Right=�$��19��$��0,8,5,13q�$��2���� -   $��Button��$��5��$��Right=�$��	0,8,25,13q�$��19��$��20��  Maximise_Btn_Style#���  $��2���� -   $��Button��$��4��$��Right=�$��	0,8,45,13q�$��19��$��20��  Minimise_Btn_Styl#���  $��2��$��2���� -    ��Name$ Connect_btn��$��Right=�$��Connect��  Connect_Btn_Style#���  $��70��$��0,42,89,241q�$��2��$��2��� -   $��progressBar1��$��1��$��2��$��10��$
��Left=�$��	0,327,0,0q�$	��Top=�$	��403��$��3���� -   $��	About_btn��$��About��$��4��$
��Left=�$	��Top=�$��2��$��32��$��73��$��257,293,0,0q�$��2���� -   $��image3��$��26��$
��Left=�$��	84,16,0,0q�$
��Fill=�$	��Top=�$	��263��$.��(/K5TUP;component/Images/Test_Utility.png��$��3��$��3���� -   $��image4��$��64��$
��Left=�$��
12,-10,0,0q�$
��Fill=�$	��Top=�$��64��$$��/K5TUP;component/Images/K5.ico��$��3���� -   $��image5��$��43��$
��Left=�$��60,7,0,0q�$
��Fill=�$	��Top=�$��40��$&�� /K5TUP;component/Images/LOGO.ico��$��4��$��3���� -   $��image6��$��4��$��43��$
��Left=�$��	104,7,0,0q�$
��Fill=�$	��Top=�$	��143��$(��"/K5TUP;component/Images/COSMED.ico��$��3���� -   $��	Store_btn��$��2��$��2��$��63��$��70��$
��Left=�$	��Top=�$��	68,42,0,0q�.���� $��Store Port Setting��$
��Wrap=�$/ )store the port settings to K5TUPInfo file���� -   $��image2��$��4��$	��258��$
��Left=�$��	90,38,0,0q�$
��Fill=�$	��Top=�$	��217��$%��/K5TUP;component/Images/K5b.png��$��2��$��2���� -   $��	Clear_btn��$��Clear��$��4��$��2��$
��Left=�$	��Top=�#���  $��32��$��73��$��	7,293,0,0q�$ clear receive box���� -   $��image1��$��3��$��2��$��64��$
��Left=�$��8,42,0,0q�$
��Fill=�$	��Top=�$��64��$%��/K5TUP;component/Images/co2.ico��