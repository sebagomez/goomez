import '../css/goomez.css';
import * as React from 'react';
import { RouteComponentProps } from 'react-router';


interface SearchState {
	searchPattern: string;
	files: string[];
	loading: boolean;
}

export class Home extends React.Component<RouteComponentProps<{}>, SearchState> {

	constructor() {
		super();
		this.state = { files: [], loading: true, searchPattern: "" };
	}

	search() {
		fetch('api/Search/Search?pattern=' + this.state.searchPattern)
			.then(response => response.json() as Promise<string[]>)
			.then(data => {
				debugger;
				this.setState({ files: data, loading: false });
			})
			.catch(error => {
				debugger;
				console.error(error);
			});
	}

	updateInputValue(evt: any) {
		this.setState({ searchPattern: evt.target.value });
	}

	public render() {
		return <div>
			<div className="bigLogo"><span className="blue">G</span><span className="red">o</span><span className="yellow">o</span><span className="blue">m</span><span className="green">e</span><span className="red z">z</span></div>
			<input value={this.state.searchPattern} onChange={evt => this.updateInputValue(evt)} />
			<button onClick={() => { this.search() }}>Search</button>
			<ul>
				{this.state.files.map(file =>
					<li>{file}</li>
				)}
			</ul>
		</div>;
	}
}
