   M S B A M L   `   `   `  ���� E  AKnockerWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:KnockerWPF
KnockerWPF      KnockerWPF.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     *localclr-namespace:KnockerWPF     -     �Name$  main_window�� � Title$ Knocker��$	��296��$	��525�� � WindowStartupLocation$ CenterScreen=� � 
ResizeMode$ NoResize=� � 	Resources    dg1 �� Property �� Value&   System.Windows.Controls.DataGrid u� Left u� Right
	 u� Top
 u� Bottom��%  &	        �� .���  � �� �	#FF2B65E2��  � ��$ 2q���  �@ ColumnHeaderStyle �� .���  � �� u� $ 0Q�$ 0Q�$	 0Q�$
 2Q���  � �� �	#FF2B65E2��  � ��$ Center=���  � ��$ Courier New)���  � ��$ 14(�.��� .+��� -    ��Name$ 	btn_knock��$��Knock to all doors!��$
��Left=�$��381,225,0,0q�$	��Top=�$	��126��$��36��$��Arial Narrow)�$��14(�  -     Name$ 
rooms_list��$
��Left=�$��	10,16,0,0q�$	��Top=�$	��185��$	��398��   CanUserReorderColumns .     SelectionMode$ Single=�   CanUserResizeRows .     AutoGenerateColumns .     
IsReadOnly .    ColumnWidth5  /System.Windows.Controls.DataGridLengthConverter$ * #��[     HeadersVisibility$ Column=�����  �� Path$ Rooms� �� Source$ Rooms�� � DataContext �� $ Rooms�$ Rooms�� *  $System.Windows.Controls.DataGridCell �� 
TargetType �� Triggers P� Property P� Value��%p   '        �� "   P�  �@ 
IsSelected . . ���  � �� �	#FFF1FB8C��  � ��
 �   �   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Room address��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� $ Address�  $ 	Room name�� �� $
 Name�  $ 
Room state�� �� $ State� �� NotifyOnSourceUpdated .  �� Mode$ TwoWay=��� -   $ btn_add��$��Add room...��$
��Left=�$��
423,16,0,0q�$	��Top=�$��84��$��33��$��Arial Narrow)�$��14(��� -   $ btn_del��$��Remove room��$
��Left=�$��
423,67,0,0q�$	��Top=�$��84��$��32��{�.  $��Arial Narrow)�$��14(�