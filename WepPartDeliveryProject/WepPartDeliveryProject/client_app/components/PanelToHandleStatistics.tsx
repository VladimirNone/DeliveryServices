import React, {  FC, useState } from 'react';
import { Col, Dropdown, Row } from 'react-bootstrap';

interface PanelToHandleUsersProps {
    statisticQueries: statisticQueryInfo[],
    buildChart: (linkQuery: statisticQueryInfo) => void;
}

const PanelToHandleStatistics: FC<PanelToHandleUsersProps> = ({ statisticQueries, buildChart }) => {
    const [selectedQuery, setSelectedQuery] = useState<statisticQueryInfo>();

    const handleSelectQuery = (eventKey: string | null): void => {
        const selQuery = statisticQueries.find(el => el.nameQuery == eventKey);
        setSelectedQuery(selQuery);
        
        if(selQuery != undefined){
            buildChart(selQuery);
        }
    }

    return (
        <>
            <Row className='g-0 m-1'>
                <Col xs='auto' className='g-0 me-1'>
                    <Dropdown onSelect={handleSelectQuery}>
                        <Dropdown.Toggle variant="success" id="dropdown-basic">
                            {selectedQuery == undefined ? "Выберете аналитический запрос" : selectedQuery.nameQuery}
                        </Dropdown.Toggle>
                        <Dropdown.Menu >
                            {statisticQueries.map((value, i) => <Dropdown.Item eventKey={value.nameQuery} key={i}>{value.nameQuery}</Dropdown.Item>)}
                        </Dropdown.Menu>
                    </Dropdown>
                </Col>
            </Row>
        </>
    );
}

export default PanelToHandleStatistics;

