import React, {  FC, useState } from 'react';
import { Col, Dropdown, Row } from 'react-bootstrap';

interface PanelToHandleUsersProps {
    orderStates: orderState[],
    selectState: (selectedState: orderState) => void;
}

const PanelToHandleOrders: FC<PanelToHandleUsersProps> = ({ orderStates, selectState }) => {
    const [selectedState, setSelectedState] = useState<orderState>(orderStates[0]);

    const handleSelectState = (eventKey: string | null): void => {
        if(eventKey != null)
        {
            const selState = orderStates.find(el => el.numberOfStage == Number.parseInt(eventKey));
        
            if(selState != undefined){
                setSelectedState(selState);
                selectState(selState);
            }
        }

    }

    return (
        <>
            <Row className='g-0 m-1'>
                <Col xs='auto' className='g-0 me-1'>
                    <Dropdown onSelect={handleSelectState}>
                        <Dropdown.Toggle variant="success" id="dropdown-basic">
                            {selectedState.nameOfState}
                        </Dropdown.Toggle>
                        <Dropdown.Menu >
                            {orderStates.map((value, i) => <Dropdown.Item eventKey={value.numberOfStage} key={i}>{value.nameOfState}</Dropdown.Item>)}
                        </Dropdown.Menu>
                    </Dropdown>
                </Col>
            </Row>
        </>
    );
}

export default PanelToHandleOrders;

