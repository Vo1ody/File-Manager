   M S B A M L   `   `   `  ����K  GFile Manager WPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:File_Manager_WPFFile_Manager_WPF  5   	   "    File_Manager_WPF.ItemDetails   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:File_Manager_WPF     5   	   -   5          � Title$  Свойсва��5   	    � SizeToContent$ Height=�5	   	   $	��500��5
   	    � WindowStartupLocation$ CenterScreen=�5   	   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5        $��FileIcon��5   1   $��5��6   $��0��6!   $��50��6A   $��10q�6P   6^   �� 5   
   $��0��6   $��1��6   $��Имя��6-   $
��Bold"�6;   �� 5   
   $��1��6   $��1��6   $��Путь��6-   $
��Bold"�6<   �� 5    
   $��2��6   $��1��6   $��Тип��6-   $
��Bold"�6;   �� 5!   
   $��3��6   $��1��6   $��Размер��6-   $
��Bold"�6>   �� 5"   
   $��4��6   $��1��6   $��Дата создания��6-   $
��Bold"�6E   �� 5$   
   -   5        $��FileName��5$   -   $��0��6   $��2��6   6-   �� 5%   
   -   5        $��FilePath��5%   -   $��1��6   $��2��6   6-   �� 5&   
   -   5        $��FileType��5&   -   $��2��6   $��2��6   6-   �� 5'   
   -   5        $��FileSize��5'   -   $��3��6   $��2��6   6-   �� 5(   
   -   5        $��FileCreationTime��5(   -   $��4��6   $��2��6   6-   �� 5*   
   $��5q�6   $��5��6    $��3��6-    �� Orientation$ 
Horizontal=�6A   $��Right=�6Z   .+�5+      �� -   5        $
��10 3q�5+      .��68   Закрыть6A   5,      5-      5.      