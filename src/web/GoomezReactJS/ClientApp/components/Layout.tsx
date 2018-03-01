import * as React from 'react';
import { NavMenu } from './NavMenu';
import { Home } from './Home';

export interface LayoutProps {
    children?: React.ReactNode;
}

export class Layout extends React.Component<LayoutProps, {}> {
	public render() {

		return <div className='col-sm-12'>
			{this.props.children}
		</div>;

        //return <div className='container-fluid'>
        //    <div className='row'>
        //        <div className='col-sm-3'>
        //            <NavMenu />
        //        </div>
        //        <div className='col-sm-9'>
        //            { this.props.children }
        //        </div>
        //    </div>
        //</div>;
    }
}
