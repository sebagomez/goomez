import '../css/goomez.css';
import * as React from 'react';
import { RouteComponentProps } from 'react-router';

export class Home extends React.Component<RouteComponentProps<{}>, {}> {
    public render() {
        return <div>
			<div className="bigLogo"><span className="blue">G</span><span className="red">o</span><span className="yellow">o</span><span className="blue">m</span><span className="green">e</span><span className="red z">z</span></div>
        </div>;
    }
}
