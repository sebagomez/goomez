import '../css/goomez.css';
import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import { ResultFile } from './ResultFile';
import { IndexedFile } from '../util/IndexedFile';

interface SearchState {
	files: IndexedFile[];
	pattern: string;
	loading: boolean;
	milliseconds: number;
}

export class Search extends React.Component<RouteComponentProps<{}>, SearchState> {

	search(pattern: string) {

		this.setState({ files: [], loading: true });

		let t0 = performance.now();
		fetch('api/Search/Search?pattern=' + pattern)
			.then(response => response.json() as Promise<IndexedFile[]>)
			.then(data => {
				let delta = performance.now() - t0;
				this.setState({ files: data, pattern: pattern, loading: false, milliseconds: delta });
			})
			.catch(error => {
				this.setState({ files: [], pattern: pattern, loading: false });
				console.error(error);
			});
	}

	updateInputValue(evt: any) {
		this.setState({ pattern: evt.target.value });
	}

	keyUpHandler(evt: any) {
		if (evt.keyCode && evt.keyCode === 13)
			this.goSearch();
	}

	componentDidMount() {
		let pattern = decodeURIComponent(this.props.location.search.substr(3));
		this.search(pattern);
	}

	goHome() {
		this.props.history.push({ pathname: '/' });
		
	}

	goSearch() {
		this.props.history.push({ pathname: '/search', search: '?q=' + this.state.pattern });
		this.search(this.state.pattern);
	}

	public render() {
		let pattern = decodeURIComponent(this.props.location.search.substr(3));

		let contents = !this.state || this.state.loading
			? <div className="resultBody smallCount">Loading resutls for {pattern}...</div>
			: this.rednderResults(this.state.files);

		return <div>
			<div className="resultHeader">
				<div className="smallLogo" title="Go home" onClick={() => this.goHome()} ><span className="blue">G</span><span className="red">o</span><span className="yellow">o</span><span className="blue">m</span><span className="green">e</span><span className="red z">z</span></div>
				<div className="inputDiv">
					<input className="bigInput" name="inputPattern" value={this.state ? this.state.pattern : ''} onChange={evt => this.updateInputValue(evt)} onKeyUp={evt => this.keyUpHandler(evt)} />
				</div>
			</div>

			{contents}
		</div>;
	}

	rednderResults(files: IndexedFile[]) {

		let count = files.length > 0 ? files.length : "No";
		let secs = this.state.milliseconds / 1000;
		let plural = files.length !== 1 ? "s" : "";

		return <div className="resultBody">
			<div className="smallCount">{count} result{plural} found ({secs} secs)</div>

			{files.map(file =>
				<ResultFile key={file.full} file={file} />
			)}
		</div>
	}
}
