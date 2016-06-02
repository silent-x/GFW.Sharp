# GFW.Sharp
A C# port of gfw.press
感谢石斑鱼大爷的原版，C#版只是为了练练手，把图形界面去掉和优化性能。完全兼容java版。</br>
ps:因为Java不会。。。

GFW.Sharp, a c# port of gfw.press. Thanks to @chinashiyu.
Parameters:

<table  cellpadding="2" cellspacing="0" >
		<tbody>
			<tr>
				<td>
					-server/-client
				</td>
				<td>
					runs under server/client mode.
				</td>
			</tr>
			<tr>
				<td>
					-lip
				</td>
				<td>
					local listen ip, default is 127.0.0.1.
				</td>
			</tr>
			<tr>
				<td>
					-lport 
				</td>
				<td>
					local listen port, default is 8558.
				</td>
			</tr>
			<tr>
				<td>
					-rip
				</td>
				<td>
					remote destination ip, must be assigned.
				</td>
			</tr>
			<tr>
				<td>
					-rport
				</td>
				<td>
					remote port ip, must be assigned.
				</td>
			</tr>
			<tr>
				<td>
					-p
				</td>
				<td>
					password, 8+ characters with at least 1 digit, 1 upper case and 1 lower case, no space is allowed.
				</td>
			</tr>
		</tbody>
	</table>

<br/>
其实还有很多要做。。。参数处理，IP有效性验证，端口是否占用。。。再说吧。。。<br />

