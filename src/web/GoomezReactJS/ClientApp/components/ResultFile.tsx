import '../css/goomez.css';
import * as React from 'react';
//import { RouteComponentProps } from 'react-router';

export interface ResultFileProps {
	file: string;
}

export class ResultFile extends React.Component<ResultFileProps , {}> {

	public render() {
		return <div>
			<li>{this.props.file}</li>
		</div>;
	}
}
