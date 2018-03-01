import '../css/goomez.css';
import * as React from 'react';
import { RouteComponentProps } from 'react-router';


interface SearchState {
	searchPattern: string;
}

export class Home extends React.Component<RouteComponentProps<{}>, SearchState> {


	search() {
		this.props.history.push({ pathname: '/search', search: '?q=' + this.state.searchPattern });
	}

	updateInputValue(evt: any) {
		this.setState({ searchPattern: evt.target.value });
	}

	keyUpHandler(evt: any) {
		if (evt.keyCode && evt.keyCode === 13)
			this.search();
	}

	public render() {
		return <div className="mainDiv">
			<div className="bigLogo"><span className="blue">G</span><span className="red">o</span><span className="yellow">o</span><span className="blue">m</span><span className="green">e</span><span className="red z">z</span></div>
			<div className="inputDiv">
				<input className="bigInput" name="inputPattern" value={this.state ? this.state.searchPattern : ''} onChange={evt => this.updateInputValue(evt)} onKeyUp={evt => this.keyUpHandler(evt)} />
			</div>
		</div>;
	}
}
