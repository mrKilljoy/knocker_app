   M S B A M L   `   `   `  ����E  AKnockerWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:KnockerWPF
KnockerWPF  5   	   O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5   	       KnockerWPF.modal_newroom   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:KnockerWPF     5   	   9System&clr-namespace:System;assembly=mscorlib     5   	   -   5          �Name$  window_newroom��5   ?    � Title$ New room properties��5	   	   $	��238��6%   $	��270��62    � 
ResizeMode$ NoResize=�6>    � WindowStartupLocation$ CenterScreen=�6T   $��2q�6y    � WindowStyle$
 None=�6�   $��
2147483640��6�   .��5
      � .+�5   
   �� -   5         ��Name$ txt_roomname��5      $
��Left=�6(   $��23��6C   $��	17,43,0,0q�6O    �� TextWrapping$
 Wrap=�6b   $	��Top=�6v   $	��231��6�    � Cursor$ IBeam��6�    � ToolTip$ Name of new room.��6�   $��
2147483642��6�   ���	#FF514E4E6�   �� 5   
   -   5        $ txt_roomadr��5      $
��Left=�6'   $��23��6B   $��
84,115,0,0q�6N   $
 Wrap=�6b   $	��Top=�6v   $	��164��6�   $ URI address of new room.��6�   $��
2147483644��6�   
����p�����	#FF514E4E6�   �� 5   
   -   5        	 ��Name$	 	btn_close��5      $��Cancel��6$   $
��Left=�65   $��158,176,0,0q�6P   $	��Top=�6e   $��84��6}   
 �� IsCancel
 . $��33��6�   $��14(�6�   $��
2147483646��6�   �� 5   
   -   5        $	 btn_ok��5      $��Apply��6!   $
��Left=�61   $��
21,176,0,0q�6L   $	��Top=�6`   $��88��6x    �� 	IsDefault . $��
2147483645��6�   $��33��6�   $��14(�6�   �� 5   
   -   5         ��Name$ lab_roomname��5      $��	Room name��6&   $
��Left=�6:   $��	17,15,0,0q�6U   $	��Top=�6h   $��12(�6�   �� 5   
   -   5        $ lab_roomadr��5      $��Room address��6%   $
��Left=�6<   $��	17,91,0,0q�6W   $	��Top=�6j   $��12(�6�   �� 5   
   -   5         ��Name$ cmb_prot_type��5      $
��Left=�6*   $��
17,115,0,0q�6E   $	��Top=�6Y   $��62��6q    �� 
IsReadOnly .  �� SelectedIndex$ 0��6�   $��25��6�   $ Protocol type��6�   $��
2147483643��6�   .@�5      ��http://��6%   ��5      https://��6&   5      5      5      