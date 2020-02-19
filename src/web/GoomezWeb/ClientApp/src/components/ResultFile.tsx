import '../goomez.css';
import * as React from 'react';
import { IndexedFile } from '../util/IndexedFile';


interface ResultFileState {
	clipboardMessage: string;
}

interface ResultFileProps {
	file: IndexedFile;
}

export class ResultFile extends React.Component<ResultFileProps, ResultFileState> {

	//https://stackoverflow.com/questions/39501289/in-reactjs-how-to-copy-text-to-clipboard
	copyToClipboard(folder: string) {
		var textField = document.createElement('textarea')
		textField.innerText = folder;
		document.body.appendChild(textField)
		textField.select()
		document.execCommand('copy')
		textField.remove()

		this.setState({ clipboardMessage: "Path copied to clipboard" });

		setTimeout(() => this.setState({ clipboardMessage: "Copy folder path" }), 1500);
	}

	public render() {
		return <div key={this.props.file.full} className="resultItem">
			<a href={'file:///' + this.props.file.full} className="file">{this.props.file.file}</a>
			<div className="full">{this.props.file.full}</div>
			<span className="size">{this.props.file.size} bytes</span>  <span onClick={() => this.copyToClipboard(this.props.file.folder)} className="folder">{this.state && this.state.clipboardMessage ? this.state.clipboardMessage : "Copy folder path"}</span>
		</div>;
	}
}
