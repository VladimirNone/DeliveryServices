import PanelToHandleStatistics from "@/components/PanelToHandleStatistics";
import BarChart from "@/components/statisticsCharts/BarChart";
import LineChart from "@/components/statisticsCharts/LineChart";
import RadarChart from "@/components/statisticsCharts/RadarChart";
import ClientLayout from "@/components/structure/ClientLayout";
import { GetStaticProps } from "next";
import { FC, useEffect, useState } from "react";
import { Col, Row } from "react-bootstrap";

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories,
        }
    }
}

const Statistics: FC<{ categories: categoryItem[] }> = ({ categories }) => {
    const [statisticQueries, setStatisticQueries] = useState<statisticQueryInfo[]>([]);
    const [selectedQuery, setSelectecQuery] = useState<statisticQueryInfo>();
    
    const handleGetQueryDataFromServer = async (query:statisticQueryInfo) => {
        setSelectecQuery(query);
    };

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/statistic/getStatisticQueries`, {
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                },
            });
            const resInfo = await resp.json() as statisticQueryInfo[];
            setStatisticQueries(resInfo);
        }
        fetchData();
    }, []);

    return (
        <ClientLayout categories={categories}>
            <PanelToHandleStatistics statisticQueries={statisticQueries} buildChart={handleGetQueryDataFromServer} />
            <Row className="d-flex justify-content-center mt-3 mb-3">
                <Col md={9}>
                    {selectedQuery != undefined && <BarChart query={selectedQuery}/>}
                </Col>
            </Row>
        </ClientLayout>
    );
}

export default Statistics;