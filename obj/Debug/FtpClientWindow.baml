   M S B A M L   `   `   `  ����K  GFile Manager WPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:File_Manager_WPFFile_Manager_WPF  5   	   &     File_Manager_WPF.FtpClientWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:File_Manager_WPF     5   	     � Title$  FTP Клиент��5   	   $	��400��6   $	��180��6(   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $
��0.3* �6   �� 5      $
��0.7* �6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� $��Адрес сервера��6   $	��3 3q�6)   �� 5   
   -   5         ��Name$
 Host��5      $��1��6    $��ftp://88.99.167.222��60   $	��3 3q�6K   �� 5   
   $��
Логин��6   $��1��6!   $	��3 3q�6.   �� 5   
   -   5        $ Login��5      $��1��6!   $��1��6.   $��nimorppu��6>   $	��3 3q�6N   �� 5   
   $��Пароль��6   $��2��6"   $	��3 3q�6/   G� 5   
   -   5         G�Name$ Password��5      $��2��6(   $��1��65    G� Password$ 
x75Ew3w6yR��6E   $	��3 3q�6[   � 5   
   $��3��6   $��2��6!   $��10, 0q�65   .+�5      �� -   5         ��Name$ btn_connect��5      $��Соединиться��6*   $	��100��6@   $��10q�6L   $��10,10q�6Y   $
��Left=�5      60   �� 5      -   5        $ 	btn_clear��5      $��Очистить��6(   $	��100��6;   $��10q�6G   $��10,10q�6T   $
��Left=�5      60   �� 5      -   5        $ 
btn_cancel��5      $��Отмена��6)   $	��100��6:   $��10q�6F   $��10,10q�6S   $
��Left=�5       60   5!      5"      5#      