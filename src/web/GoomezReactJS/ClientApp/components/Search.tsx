import '../css/goomez.css';
import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import { ResultFile } from './ResultFile';
import { IndexedFile } from '../util/IndexedFile';

interface SearchState {
	files: IndexedFile[];
	pattern: string;
	loading: boolean;
}

export class Search extends React.Component<RouteComponentProps<{}>, SearchState> {

	constructor() {
		super();
		this.setState({ loading: true });
	}

	search(pattern: string) {
		fetch('api/Search/Search?pattern=' + pattern)
			.then(response => response.json() as Promise<IndexedFile[]>)
			.then(data => {
				this.setState({ files: data, pattern: pattern, loading: false });
			})
			.catch(error => {
				console.error(error);
			});
	}

	public render() {
		let pattern = this.props.location.search.substr(3);

		if (!this.state || this.state.loading) {
			this.search(pattern);
			return <div>loading...</div>;
		}

		return <div>
			<h1>The search result page for {this.state.pattern}</h1>
			{this.state.files.map(file =>
				<ResultFile file={file} />
			)}
		</div>;



	}
}
