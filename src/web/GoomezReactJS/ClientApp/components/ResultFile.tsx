import '../css/goomez.css';
import * as React from 'react';
import { IndexedFile } from '../util/IndexedFile';


export interface ResultFileProps {
	file: IndexedFile;
}

export class ResultFile extends React.Component<ResultFileProps , {}> {

	public render() {
		return <div>
			<p key={this.props.file.full}>{this.props.file.full}</p>
		</div>;
	}
}
